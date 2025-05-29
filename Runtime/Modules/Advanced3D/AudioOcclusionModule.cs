using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux.Modules.Advanced3D
{
    /// <summary>
    /// Module that provides audio occlusion and obstruction simulation
    /// </summary>
    [AudioFluxModule(
        "advanced3d.occlusion",
        "Audio Occlusion",
        "1.0.0",
        Category = "Advanced3D",
        Description = "Simulates sound being blocked by walls and objects",
        Author = "LiteNinja"
    )]
    public class AudioOcclusionModule : SFXServiceModuleBase
    {
        [ModuleSetting("Max Occlusion Distance", Description = "Maximum distance to check for occlusion")]
        public float MaxOcclusionDistance { get; set; } = 100f;

        [ModuleSetting("Occlusion Layers", Description = "Physics layers that can occlude audio")]
        public LayerMask OcclusionLayers { get; set; } = -1;

        [ModuleSetting("Update Frequency", Description = "How often to update occlusion (Hz)")]
        public float UpdateFrequency { get; set; } = 10f;

        [ModuleSetting("Min Occlusion Factor", Description = "Minimum volume multiplier when fully occluded")]
        public float MinOcclusionFactor { get; set; } = 0.1f;

        [ModuleSetting("Low Pass Frequency", Description = "Low pass filter frequency when occluded")]
        public float OccludedLowPassFrequency { get; set; } = 1000f;

        public override string ModuleId => "advanced3d.occlusion";
        public override string ModuleName => "Audio Occlusion";
        public override Version ModuleVersion => new Version(1, 0, 0);

        private readonly Dictionary<SoundHandle, OcclusionData> _occlusionData = new Dictionary<SoundHandle, OcclusionData>();
        private float _lastUpdateTime;
        private Camera _listenerCamera;

        private struct OcclusionData
        {
            public float occlusionFactor;
            public bool isOccluded;
            public Vector3 lastSourcePosition;
            public AudioLowPassFilter lowPassFilter;
        }

        protected override void OnInitialize()
        {
            // Find the main camera as the listener reference
            _listenerCamera = Camera.main;
            if (_listenerCamera == null)
            {
                _listenerCamera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            // Subscribe to advanced audio events
            AdvancedAudioEvents.OnAudioOcclusionRequested += HandleOcclusionRequest;

            LogInfo("Audio Occlusion Module initialized");
        }

        protected override void OnDestroy()
        {
            AdvancedAudioEvents.OnAudioOcclusionRequested -= HandleOcclusionRequest;
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (Time.time - _lastUpdateTime >= 1f / UpdateFrequency)
            {
                UpdateOcclusion();
                _lastUpdateTime = Time.time;
            }
        }

        protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
        {
            if (handle?.audioSource != null && handle.audioSource.spatialBlend > 0.5f)
            {
                // Only apply occlusion to 3D sounds
                InitializeOcclusionForSound(handle);
            }
        }

        protected override void OnSoundStoppedInternal(string soundId, SoundHandle handle)
        {
            CleanupOcclusionForSound(handle);
        }

        protected override void OnSoundCompletedInternal(string soundId, SoundHandle handle)
        {
            CleanupOcclusionForSound(handle);
        }

        private void InitializeOcclusionForSound(SoundHandle handle)
        {
            if (handle?.audioSource == null)
                return;

            var audioSource = handle.audioSource;

            // Add low pass filter if not present
            var lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
                lowPassFilter.cutoffFrequency = 22000f; // Start with no filtering
            }

            _occlusionData[handle] = new OcclusionData
            {
                occlusionFactor = 1f,
                isOccluded = false,
                lastSourcePosition = audioSource.transform.position,
                lowPassFilter = lowPassFilter
            };
        }

        private void CleanupOcclusionForSound(SoundHandle handle)
        {
            if (_occlusionData.TryGetValue(handle, out var data))
            {
                // Remove the low pass filter if we added it
                if (data.lowPassFilter != null)
                {
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(data.lowPassFilter);
                    }
                }

                _occlusionData.Remove(handle);
            }
        }

        private void UpdateOcclusion()
        {
            if (_listenerCamera == null)
                return;

            var listenerPosition = _listenerCamera.transform.position;
            var soundsToRemove = new List<SoundHandle>();

            foreach (var kvp in _occlusionData)
            {
                var handle = kvp.Key;
                var data = kvp.Value;

                if (handle?.audioSource == null || !handle.audioSource.isPlaying)
                {
                    soundsToRemove.Add(handle);
                    continue;
                }

                var sourcePosition = handle.audioSource.transform.position;
                var distance = Vector3.Distance(listenerPosition, sourcePosition);

                if (distance <= MaxOcclusionDistance)
                {
                    var occlusionFactor = CalculateOcclusion(listenerPosition, sourcePosition);
                    ApplyOcclusion(handle, data, occlusionFactor);
                }
            }

            // Clean up sounds that are no longer playing
            foreach (var handle in soundsToRemove)
            {
                CleanupOcclusionForSound(handle);
            }
        }

        private float CalculateOcclusion(Vector3 listenerPos, Vector3 sourcePos)
        {
            var direction = sourcePos - listenerPos;
            var distance = direction.magnitude;

            if (distance < 0.1f)
                return 1f; // No occlusion for very close sounds

            // Perform raycast to check for obstacles
            if (UnityEngine.Physics.Raycast(listenerPos, direction.normalized, out RaycastHit hit, distance, OcclusionLayers))
            {
                // Calculate occlusion based on hit distance vs total distance
                var occlusionStrength = hit.distance / distance;
                return Mathf.Lerp(MinOcclusionFactor, 1f, occlusionStrength);
            }

            return 1f; // No occlusion
        }

        private void ApplyOcclusion(SoundHandle handle, OcclusionData data, float occlusionFactor)
        {
            if (handle?.audioSource == null)
                return;

            // Update occlusion data
            var newData = data;
            newData.occlusionFactor = occlusionFactor;
            newData.isOccluded = occlusionFactor < 0.95f;
            _occlusionData[handle] = newData;

            // Apply volume occlusion
            var baseVolume = handle.audioSource.volume / (data.occlusionFactor > 0 ? data.occlusionFactor : 1f);
            handle.audioSource.volume = baseVolume * occlusionFactor;

            // Apply low pass filter for muffled effect
            if (data.lowPassFilter != null)
            {
                var targetFrequency = newData.isOccluded
                    ? Mathf.Lerp(OccludedLowPassFrequency, 22000f, occlusionFactor)
                    : 22000f;

                data.lowPassFilter.cutoffFrequency = targetFrequency;
            }
        }

        private void HandleOcclusionRequest(Vector3 sourcePos, Vector3 listenerPos, SoundHandle handle)
        {
            if (handle?.audioSource == null)
                return;

            var occlusionFactor = CalculateOcclusion(listenerPos, sourcePos);

            if (_occlusionData.TryGetValue(handle, out var data))
            {
                ApplyOcclusion(handle, data, occlusionFactor);
            }
        }

        /// <summary>
        /// Manually request occlusion calculation for a specific sound
        /// </summary>
        public void RequestOcclusion(SoundHandle handle, Vector3? listenerPosition = null)
        {
            if (handle?.audioSource == null)
                return;

            var listenerPos = listenerPosition ?? (_listenerCamera?.transform.position ?? Vector3.zero);
            var sourcePos = handle.audioSource.transform.position;

            AdvancedAudioEvents.InvokeAudioOcclusionRequested(sourcePos, listenerPos, handle);
        }

        /// <summary>
        /// Get the current occlusion factor for a sound
        /// </summary>
        public float GetOcclusionFactor(SoundHandle handle)
        {
            return _occlusionData.TryGetValue(handle, out var data) ? data.occlusionFactor : 1f;
        }

        /// <summary>
        /// Check if a sound is currently occluded
        /// </summary>
        public bool IsOccluded(SoundHandle handle)
        {
            return _occlusionData.TryGetValue(handle, out var data) && data.isOccluded;
        }
    }
}