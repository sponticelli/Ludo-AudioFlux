using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux.Modules.DynamicMusic
{
    /// <summary>
    /// Module that provides state-based music transitions and intensity scaling
    /// </summary>
    [AudioFluxModule(
        "dynamic.statemusic",
        "State-Based Music",
        "1.0.0",
        Category = "DynamicMusic",
        Description = "Smooth music changes based on game state with intensity scaling",
        Author = "LiteNinja"
    )]
    public class StateBasedMusicModule : MusicServiceModuleBase
    {
        [ModuleSetting("Default Transition Time", Description = "Default time for state transitions")]
        public float DefaultTransitionTime { get; set; } = 2f;

        [ModuleSetting("Intensity Smoothing", Description = "How smoothly intensity changes are applied")]
        public float IntensitySmoothing { get; set; } = 5f;

        [ModuleSetting("Auto Transition", Description = "Automatically transition between states")]
        public bool AutoTransition { get; set; } = true;

        public override string ModuleId => "dynamic.statemusic";
        public override string ModuleName => "State-Based Music";
        public override Version ModuleVersion => new Version(1, 0, 0);

        private readonly Dictionary<string, MusicState> _musicStates = new Dictionary<string, MusicState>();
        private readonly Dictionary<string, StateTransition> _transitions = new Dictionary<string, StateTransition>();
        private string _currentState = "default";
        private float _currentIntensity = 0.5f;
        private float _targetIntensity = 0.5f;
        private MusicHandle _currentMusicHandle;

        [Serializable]
        public class MusicState
        {
            public string stateId;
            public string musicId;
            public float baseVolume = 1f;
            public float basePitch = 1f;
            public bool enableLayers = true;
            public float[] layerIntensityThresholds = new float[0];
            public string[] layerMusicIds = new string[0];
        }

        [Serializable]
        public class StateTransition
        {
            public string fromState;
            public string toState;
            public float transitionTime = 2f;
            public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            public bool crossfade = true;
        }

        protected override void OnInitialize()
        {
            // Subscribe to game state events
            AdvancedAudioEvents.OnGameStateChanged += HandleGameStateChanged;
            AdvancedAudioEvents.OnMusicIntensityChanged += HandleIntensityChanged;

            // Initialize default state
            RegisterDefaultStates();

            LogInfo("State-Based Music Module initialized");
        }

        protected override void OnDestroy()
        {
            AdvancedAudioEvents.OnGameStateChanged -= HandleGameStateChanged;
            AdvancedAudioEvents.OnMusicIntensityChanged -= HandleIntensityChanged;
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            UpdateIntensity();
        }

        private void RegisterDefaultStates()
        {
            // Register some common game states
            RegisterMusicState(new MusicState
            {
                stateId = "menu",
                musicId = "menu_music",
                baseVolume = 0.8f,
                enableLayers = false
            });

            RegisterMusicState(new MusicState
            {
                stateId = "exploration",
                musicId = "exploration_music",
                baseVolume = 0.6f,
                enableLayers = true,
                layerIntensityThresholds = new float[] { 0.3f, 0.6f, 0.8f },
                layerMusicIds = new string[] { "exploration_ambient", "exploration_tension", "exploration_action" }
            });

            RegisterMusicState(new MusicState
            {
                stateId = "combat",
                musicId = "combat_music",
                baseVolume = 1f,
                enableLayers = true,
                layerIntensityThresholds = new float[] { 0.2f, 0.5f, 0.8f },
                layerMusicIds = new string[] { "combat_low", "combat_medium", "combat_high" }
            });

            // Register transitions
            RegisterTransition("menu", "exploration", 3f, true);
            RegisterTransition("exploration", "combat", 1f, true);
            RegisterTransition("combat", "exploration", 2f, true);
            RegisterTransition("exploration", "menu", 2f, true);
        }

        /// <summary>
        /// Register a music state
        /// </summary>
        public void RegisterMusicState(MusicState state)
        {
            if (state == null || string.IsNullOrEmpty(state.stateId))
                return;

            _musicStates[state.stateId] = state;
            LogInfo($"Registered music state: {state.stateId}");
        }

        /// <summary>
        /// Register a state transition
        /// </summary>
        public void RegisterTransition(string fromState, string toState, float transitionTime, bool crossfade = true)
        {
            var transitionKey = $"{fromState}->{toState}";
            _transitions[transitionKey] = new StateTransition
            {
                fromState = fromState,
                toState = toState,
                transitionTime = transitionTime,
                crossfade = crossfade
            };

            LogInfo($"Registered transition: {transitionKey}");
        }

        /// <summary>
        /// Transition to a specific music state
        /// </summary>
        public void TransitionToState(string stateId, float? customTransitionTime = null)
        {
            if (!_musicStates.TryGetValue(stateId, out var targetState))
            {
                LogWarning($"Music state '{stateId}' not found");
                return;
            }

            if (_currentState == stateId)
                return;

            var transitionKey = $"{_currentState}->{stateId}";
            var transition = _transitions.GetValueOrDefault(transitionKey);
            var transitionTime = customTransitionTime ?? transition?.transitionTime ?? DefaultTransitionTime;
            var shouldCrossfade = transition?.crossfade ?? true;

            LogInfo($"Transitioning from '{_currentState}' to '{stateId}' (time: {transitionTime}s, crossfade: {shouldCrossfade})");

            if (shouldCrossfade && !string.IsNullOrEmpty(targetState.musicId))
            {
                var playParams = CreatePlayParamsForState(targetState);
                MusicService.CrossfadeTo(targetState.musicId, transitionTime, playParams);
                // Note: CrossfadeTo doesn't return a handle, we'll get it from the event
            }
            else if (!string.IsNullOrEmpty(targetState.musicId))
            {
                MusicService.StopMusic(transitionTime * 0.5f);
                var playParams = CreatePlayParamsForState(targetState);
                _currentMusicHandle = MusicService.PlayMusic(targetState.musicId, playParams);
            }

            _currentState = stateId;
        }

        /// <summary>
        /// Set the current music intensity (0.0 to 1.0)
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _targetIntensity = Mathf.Clamp01(intensity);
            AdvancedAudioEvents.InvokeMusicIntensityChanged(_targetIntensity);
        }

        /// <summary>
        /// Get the current music state
        /// </summary>
        public string GetCurrentState()
        {
            return _currentState;
        }

        /// <summary>
        /// Get the current intensity
        /// </summary>
        public float GetCurrentIntensity()
        {
            return _currentIntensity;
        }

        private MusicPlayParams CreatePlayParamsForState(MusicState state)
        {
            var builder = new MusicPlayParamsBuilder()
                .WithVolume(state.baseVolume)
                .WithPitch(state.basePitch)
                .WithLayers(state.enableLayers);

            // Apply intensity-based layer volumes
            if (state.enableLayers && state.layerIntensityThresholds.Length > 0)
            {
                var layerVolumes = CalculateLayerVolumes(state);
                builder.WithLayerVolumes(layerVolumes);
            }

            return builder.Build();
        }

        private float[] CalculateLayerVolumes(MusicState state)
        {
            var layerVolumes = new float[state.layerIntensityThresholds.Length];

            for (int i = 0; i < state.layerIntensityThresholds.Length; i++)
            {
                var threshold = state.layerIntensityThresholds[i];

                if (_currentIntensity >= threshold)
                {
                    // Calculate volume based on how much we exceed the threshold
                    var nextThreshold = i < state.layerIntensityThresholds.Length - 1
                        ? state.layerIntensityThresholds[i + 1]
                        : 1f;

                    var range = nextThreshold - threshold;
                    var progress = range > 0 ? (_currentIntensity - threshold) / range : 1f;
                    layerVolumes[i] = Mathf.Clamp01(progress);
                }
                else
                {
                    layerVolumes[i] = 0f;
                }
            }

            return layerVolumes;
        }

        private void UpdateIntensity()
        {
            if (Mathf.Abs(_currentIntensity - _targetIntensity) > 0.01f)
            {
                _currentIntensity = Mathf.Lerp(_currentIntensity, _targetIntensity, Time.deltaTime * IntensitySmoothing);

                // Update layer volumes based on new intensity
                if (_musicStates.TryGetValue(_currentState, out var currentState) &&
                    currentState.enableLayers &&
                    _currentMusicHandle != null)
                {
                    UpdateLayerVolumes(currentState);
                }
            }
        }

        private void UpdateLayerVolumes(MusicState state)
        {
            if (_currentMusicHandle?.layerSources == null)
                return;

            var layerVolumes = CalculateLayerVolumes(state);

            for (int i = 0; i < Mathf.Min(layerVolumes.Length, _currentMusicHandle.layerSources.Count); i++)
            {
                var layerSource = _currentMusicHandle.layerSources[i];
                if (layerSource != null)
                {
                    layerSource.volume = layerVolumes[i] * state.baseVolume;
                }
            }
        }

        private void HandleGameStateChanged(string stateName, object stateData)
        {
            if (AutoTransition && _musicStates.ContainsKey(stateName))
            {
                TransitionToState(stateName);
            }
        }

        private void HandleIntensityChanged(float intensity)
        {
            SetIntensity(intensity);
        }

        protected override void OnMusicStartedInternal(string musicId, MusicHandle handle)
        {
            _currentMusicHandle = handle;
        }

        protected override void OnMusicStoppedInternal(string musicId, MusicHandle handle)
        {
            if (_currentMusicHandle == handle)
            {
                _currentMusicHandle = null;
            }
        }
    }
}