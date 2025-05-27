using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux.Modules.Performance
{
    /// <summary>
    /// Module that provides Level of Detail (LOD) system for audio based on distance and importance
    /// </summary>
    [AudioFluxModule(
        "performance.audiolod",
        "Audio LOD System",
        "1.0.0",
        Category = "Performance",
        Description = "Lower quality audio at greater distances for better performance",
        Author = "LiteNinja"
    )]
    public class AudioLODModule : SFXServiceModuleBase
    {
        [ModuleSetting("LOD Update Frequency", Description = "How often to update LOD calculations (Hz)")]
        public float LODUpdateFrequency { get; set; } = 5f;

        [ModuleSetting("Near Distance", Description = "Distance for highest quality audio")]
        public float NearDistance { get; set; } = 10f;

        [ModuleSetting("Medium Distance", Description = "Distance for medium quality audio")]
        public float MediumDistance { get; set; } = 30f;

        [ModuleSetting("Far Distance", Description = "Distance for low quality audio")]
        public float FarDistance { get; set; } = 100f;

        [ModuleSetting("Cull Distance", Description = "Distance beyond which audio is culled")]
        public float CullDistance { get; set; } = 200f;

        [ModuleSetting("High Quality Sample Rate", Description = "Sample rate for high quality audio")]
        public int HighQualitySampleRate { get; set; } = 44100;

        [ModuleSetting("Medium Quality Sample Rate", Description = "Sample rate for medium quality audio")]
        public int MediumQualitySampleRate { get; set; } = 22050;

        [ModuleSetting("Low Quality Sample Rate", Description = "Sample rate for low quality audio")]
        public int LowQualitySampleRate { get; set; } = 11025;

        public override string ModuleId => "performance.audiolod";
        public override string ModuleName => "Audio LOD System";
        public override Version ModuleVersion => new Version(1, 0, 0);

        private readonly Dictionary<SoundHandle, AudioLODData> _lodData = new Dictionary<SoundHandle, AudioLODData>();
        private float _lastLODUpdate;
        private Camera _listenerCamera;

        public enum AudioLODLevel
        {
            High = 0,
            Medium = 1,
            Low = 2,
            Culled = 3
        }

        private struct AudioLODData
        {
            public AudioLODLevel currentLOD;
            public AudioLODLevel targetLOD;
            public float distance;
            public int priority;
            public bool isImportant;
            public AudioClip originalClip;
            public Dictionary<AudioLODLevel, AudioClip> lodClips;
        }

        protected override void OnInitialize()
        {
            // Find the main camera as the listener reference
            _listenerCamera = Camera.main;
            if (_listenerCamera == null)
            {
                _listenerCamera = UnityEngine.Object.FindObjectOfType<Camera>();
            }

            LogInfo("Audio LOD Module initialized");
        }

        protected override void OnUpdate()
        {
            if (Time.time - _lastLODUpdate >= 1f / LODUpdateFrequency)
            {
                UpdateAudioLOD();
                _lastLODUpdate = Time.time;
            }
        }

        protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
        {
            if (handle?.audioSource != null && handle.audioSource.spatialBlend > 0.5f)
            {
                // Only apply LOD to 3D sounds
                InitializeLODForSound(handle);
            }
        }

        protected override void OnSoundStoppedInternal(string soundId, SoundHandle handle)
        {
            CleanupLODForSound(handle);
        }

        protected override void OnSoundCompletedInternal(string soundId, SoundHandle handle)
        {
            CleanupLODForSound(handle);
        }

        private void InitializeLODForSound(SoundHandle handle)
        {
            if (handle?.audioSource == null)
                return;

            var audioSource = handle.audioSource;
            var originalClip = audioSource.clip;

            if (originalClip == null)
                return;

            // Determine if this sound is important (high priority)
            var isImportant = audioSource.priority < 64; // Unity's priority system (0-256, lower is higher priority)

            var lodData = new AudioLODData
            {
                currentLOD = AudioLODLevel.High,
                targetLOD = AudioLODLevel.High,
                distance = 0f,
                priority = audioSource.priority,
                isImportant = isImportant,
                originalClip = originalClip,
                lodClips = new Dictionary<AudioLODLevel, AudioClip>()
            };

            // Store original clip as high quality
            lodData.lodClips[AudioLODLevel.High] = originalClip;

            // Generate LOD clips if needed (in a real implementation, these would be pre-generated)
            GenerateLODClips(ref lodData);

            _lodData[handle] = lodData;
        }

        private void GenerateLODClips(ref AudioLODData lodData)
        {
            // In a real implementation, you would have pre-generated LOD clips
            // For this example, we'll just reference the original clip for all LOD levels
            // In practice, you'd want to:
            // 1. Pre-process audio files at different sample rates
            // 2. Store them as separate assets
            // 3. Load the appropriate one based on LOD level

            lodData.lodClips[AudioLODLevel.Medium] = lodData.originalClip;
            lodData.lodClips[AudioLODLevel.Low] = lodData.originalClip;
        }

        private void CleanupLODForSound(SoundHandle handle)
        {
            _lodData.Remove(handle);
        }

        private void UpdateAudioLOD()
        {
            if (_listenerCamera == null)
                return;

            var listenerPosition = _listenerCamera.transform.position;
            var soundsToRemove = new List<SoundHandle>();

            foreach (var kvp in _lodData)
            {
                var handle = kvp.Key;
                var lodData = kvp.Value;

                if (handle?.audioSource == null || !handle.audioSource.isPlaying)
                {
                    soundsToRemove.Add(handle);
                    continue;
                }

                var sourcePosition = handle.audioSource.transform.position;
                var distance = Vector3.Distance(listenerPosition, sourcePosition);

                // Update distance
                var updatedLodData = lodData;
                updatedLodData.distance = distance;

                // Calculate target LOD level
                var targetLOD = CalculateLODLevel(distance, lodData.isImportant);
                updatedLodData.targetLOD = targetLOD;

                // Apply LOD if it changed
                if (updatedLodData.currentLOD != targetLOD)
                {
                    ApplyLOD(handle, ref updatedLodData);
                }

                _lodData[handle] = updatedLodData;
            }

            // Clean up sounds that are no longer playing
            foreach (var handle in soundsToRemove)
            {
                CleanupLODForSound(handle);
            }
        }

        private AudioLODLevel CalculateLODLevel(float distance, bool isImportant)
        {
            // Important sounds get better LOD treatment
            var nearDist = isImportant ? NearDistance * 1.5f : NearDistance;
            var mediumDist = isImportant ? MediumDistance * 1.5f : MediumDistance;
            var farDist = isImportant ? FarDistance * 1.5f : FarDistance;
            var cullDist = isImportant ? CullDistance * 2f : CullDistance;

            if (distance > cullDist)
                return AudioLODLevel.Culled;
            else if (distance > farDist)
                return AudioLODLevel.Low;
            else if (distance > mediumDist)
                return AudioLODLevel.Medium;
            else
                return AudioLODLevel.High;
        }

        private void ApplyLOD(SoundHandle handle, ref AudioLODData lodData)
        {
            if (handle?.audioSource == null)
                return;

            var audioSource = handle.audioSource;
            var targetLOD = lodData.targetLOD;

            switch (targetLOD)
            {
                case AudioLODLevel.Culled:
                    // Stop the sound entirely
                    audioSource.Stop();
                    LogInfo($"Culled audio at distance {lodData.distance:F1}m");
                    break;

                case AudioLODLevel.Low:
                    ApplyLowQualityLOD(audioSource, lodData);
                    break;

                case AudioLODLevel.Medium:
                    ApplyMediumQualityLOD(audioSource, lodData);
                    break;

                case AudioLODLevel.High:
                    ApplyHighQualityLOD(audioSource, lodData);
                    break;
            }

            lodData.currentLOD = targetLOD;
        }

        private void ApplyHighQualityLOD(AudioSource audioSource, AudioLODData lodData)
        {
            // Use original clip and settings
            if (lodData.lodClips.TryGetValue(AudioLODLevel.High, out var clip))
            {
                if (audioSource.clip != clip)
                {
                    var currentTime = audioSource.time;
                    audioSource.clip = clip;
                    audioSource.time = currentTime;
                }
            }

            // Remove any quality-reducing filters
            RemoveQualityFilters(audioSource);
        }

        private void ApplyMediumQualityLOD(AudioSource audioSource, AudioLODData lodData)
        {
            // Apply medium quality settings
            if (lodData.lodClips.TryGetValue(AudioLODLevel.Medium, out var clip))
            {
                if (audioSource.clip != clip)
                {
                    var currentTime = audioSource.time;
                    audioSource.clip = clip;
                    audioSource.time = currentTime;
                }
            }

            // Add subtle low-pass filter
            ApplyQualityFilter(audioSource, 8000f);
        }

        private void ApplyLowQualityLOD(AudioSource audioSource, AudioLODData lodData)
        {
            // Apply low quality settings
            if (lodData.lodClips.TryGetValue(AudioLODLevel.Low, out var clip))
            {
                if (audioSource.clip != clip)
                {
                    var currentTime = audioSource.time;
                    audioSource.clip = clip;
                    audioSource.time = currentTime;
                }
            }

            // Add more aggressive low-pass filter
            ApplyQualityFilter(audioSource, 4000f);
        }

        private void ApplyQualityFilter(AudioSource audioSource, float cutoffFrequency)
        {
            var lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter == null)
            {
                lowPassFilter = audioSource.gameObject.AddComponent<AudioLowPassFilter>();
            }

            lowPassFilter.cutoffFrequency = cutoffFrequency;
        }

        private void RemoveQualityFilters(AudioSource audioSource)
        {
            var lowPassFilter = audioSource.GetComponent<AudioLowPassFilter>();
            if (lowPassFilter != null)
            {
                lowPassFilter.cutoffFrequency = 22000f; // Full frequency range
            }
        }

        /// <summary>
        /// Get the current LOD level for a sound
        /// </summary>
        public AudioLODLevel GetLODLevel(SoundHandle handle)
        {
            return _lodData.TryGetValue(handle, out var data) ? data.currentLOD : AudioLODLevel.High;
        }

        /// <summary>
        /// Get the distance of a sound from the listener
        /// </summary>
        public float GetSoundDistance(SoundHandle handle)
        {
            return _lodData.TryGetValue(handle, out var data) ? data.distance : 0f;
        }

        /// <summary>
        /// Force a specific LOD level for a sound (overrides automatic calculation)
        /// </summary>
        public void ForceLODLevel(SoundHandle handle, AudioLODLevel lodLevel)
        {
            if (_lodData.TryGetValue(handle, out var data))
            {
                data.targetLOD = lodLevel;
                ApplyLOD(handle, ref data);
                _lodData[handle] = data;
            }
        }

        /// <summary>
        /// Mark a sound as important (gets better LOD treatment)
        /// </summary>
        public void SetSoundImportance(SoundHandle handle, bool isImportant)
        {
            if (_lodData.TryGetValue(handle, out var data))
            {
                data.isImportant = isImportant;
                _lodData[handle] = data;
            }
        }
    }
}