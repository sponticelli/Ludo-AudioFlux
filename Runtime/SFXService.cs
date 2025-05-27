using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux
{
    public class SfxService : MonoBehaviour, ISFXService
    {
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        [Header("Sound Library")]
        [SerializeField] private SoundDefinition[] soundLibrary;

        private readonly Dictionary<string, SoundDefinition> _soundDefinitions = new Dictionary<string, SoundDefinition>();
        private readonly Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();
        private readonly List<AudioSource> _activeAudioSources = new List<AudioSource>();
        private readonly List<SoundHandle> _activeSounds = new List<SoundHandle>();
        private readonly Dictionary<string, AudioClip> _preloadedClips = new Dictionary<string, AudioClip>();

        private float _globalVolume = 1f;
        private bool _isPaused;

        private void Awake()
        {
            InitializePool();
            RegisterSoundLibrary();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateAudioSource();
            }
        }

        private void RegisterSoundLibrary()
        {
            foreach (var soundDef in soundLibrary)
            {
                if (soundDef != null)
                {
                    RegisterSound(soundDef.name, soundDef);
                }
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject audioObj = new GameObject("PooledAudioSource");
            audioObj.transform.SetParent(transform);
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            _audioSourcePool.Enqueue(audioSource);
            return audioSource;
        }

        private AudioSource GetAudioSource()
        {
            if (_audioSourcePool.Count == 0)
            {
                if (_activeAudioSources.Count < maxPoolSize)
                {
                    CreateAudioSource();
                }
                else
                {
                    // Find and stop the lowest priority sound
                    AudioSource lowestPriority = FindLowestPrioritySource();
                    if (lowestPriority != null)
                    {
                        StopAudioSource(lowestPriority);
                    }
                    else
                    {
                        return null; // Pool exhausted
                    }
                }
            }

            AudioSource audioSource = _audioSourcePool.Dequeue();
            _activeAudioSources.Add(audioSource);
            return audioSource;
        }

        private AudioSource FindLowestPrioritySource()
        {
            AudioSource lowest = null;
            int lowestPriority = int.MaxValue;

            foreach (var source in _activeAudioSources)
            {
                if (source.isPlaying && source.priority < lowestPriority)
                {
                    lowest = source;
                    lowestPriority = source.priority;
                }
            }

            return lowest;
        }

        private void ReturnAudioSource(AudioSource audioSource)
        {
            if (audioSource == null) return;

            audioSource.Stop();
            audioSource.clip = null;
            audioSource.transform.position = Vector3.zero;
            audioSource.transform.SetParent(transform);

            _activeAudioSources.Remove(audioSource);
            _audioSourcePool.Enqueue(audioSource);
        }

        public void RegisterSound(string id, SoundDefinition soundDefinition)
        {
            _soundDefinitions[id] = soundDefinition;
        }

        public void PreloadSound(string soundId)
        {
            if (_soundDefinitions.TryGetValue(soundId, out SoundDefinition soundDef) && soundDef.ClipCount != 0)
            {
                _preloadedClips[soundId] = soundDef.AudioClip;
            }
        }

        public SoundHandle PlaySound(string soundId, SoundPlayParams playParams = default)
        {
            if (!_soundDefinitions.TryGetValue(soundId, out SoundDefinition soundDef))
            {
                Debug.LogWarning($"Sound '{soundId}' not found in sound definitions");
                return null;
            }

            return PlaySound(soundDef, playParams);
        }

        public SoundHandle PlaySound(SoundDefinition soundDef, SoundPlayParams playParams = default)
        {
            if (soundDef?.AudioClip == null)
            {
                Debug.LogWarning("SoundDefinition or AudioClip is null");
                return null;
            }

            AudioSource audioSource = GetAudioSource();
            if (audioSource == null)
            {
                Debug.LogWarning("No available audio sources in pool");
                return null;
            }

            // Configure audio source
            ConfigureAudioSource(audioSource, soundDef, playParams);

            // Create sound handle
            SoundHandle handle = new SoundHandle
            {
                soundId = soundDef.name,
                audioSource = audioSource,
                onComplete = playParams.onComplete
            };

            _activeSounds.Add(handle);

            // Start playback
            audioSource.Play();

            // Handle fading and completion
            if (soundDef.FadeInDuration > 0)
            {
                handle.fadeCoroutine = StartCoroutine(FadeIn(audioSource, soundDef.FadeInDuration, audioSource.volume));
            }

            StartCoroutine(MonitorSoundCompletion(handle));

            SoundEvents.InvokeSoundStarted(soundDef.name, handle);

            return handle;
        }

        private void ConfigureAudioSource(AudioSource audioSource, SoundDefinition soundDef, SoundPlayParams playParams)
        {
            audioSource.clip = soundDef.AudioClip;
            audioSource.volume = soundDef.Volume * _globalVolume * (playParams.volumeMultiplier ?? 1f);
            audioSource.pitch = soundDef.Pitch * (playParams.pitchMultiplier ?? 1f);
            audioSource.loop = playParams.loop ?? soundDef.Loop;
            audioSource.priority = soundDef.priority;
            audioSource.spatialBlend = soundDef.SpatialBlend;
            audioSource.rolloffMode = soundDef.RolloffMode;
            audioSource.maxDistance = soundDef.MaxDistance;
            audioSource.outputAudioMixerGroup = soundDef.MixerGroup;

            // Handle positioning using the new enum-based approach
            switch (playParams.PositionMode)
            {
                case SoundPositionMode.AtPosition:
                    audioSource.transform.position = playParams.Position;
                    break;

                case SoundPositionMode.FollowTarget:
                    if (playParams.FollowTarget != null)
                    {
                        audioSource.transform.SetParent(playParams.FollowTarget);
                        audioSource.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        Debug.LogWarning("FollowTarget is null but PositionMode is set to FollowTarget");
                    }
                    break;

                case SoundPositionMode.Default:
                default:
                    // No special positioning - audio source remains at service position
                    break;
            }
        }

        private IEnumerator MonitorSoundCompletion(SoundHandle handle)
        {
            while (handle.audioSource != null && handle.audioSource.isPlaying)
            {
                yield return null;
            }

            CompleteSoundHandle(handle);
        }

        private void CompleteSoundHandle(SoundHandle handle)
        {
            if (handle == null) return;

            _activeSounds.Remove(handle);

            if (handle.fadeCoroutine != null)
            {
                StopCoroutine(handle.fadeCoroutine);
            }

            SoundEvents.InvokeSoundCompleted(handle.soundId, handle);
            handle.onComplete?.Invoke(handle);

            ReturnAudioSource(handle.audioSource);
        }

        public void StopSound(SoundHandle handle, float fadeOutTime = 0f)
        {
            if (handle?.audioSource == null) return;

            if (fadeOutTime > 0f)
            {
                if (handle.fadeCoroutine != null)
                {
                    StopCoroutine(handle.fadeCoroutine);
                }

                handle.fadeCoroutine = StartCoroutine(FadeOutAndStop(handle, fadeOutTime));
            }
            else
            {
                StopAudioSource(handle.audioSource);
                SoundEvents.InvokeSoundStopped(handle.soundId, handle);
                CompleteSoundHandle(handle);
            }
        }

        private void StopAudioSource(AudioSource audioSource)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        public void StopAllSounds(float fadeOutTime = 0f)
        {
            var soundsToStop = new List<SoundHandle>(_activeSounds);
            foreach (var handle in soundsToStop)
            {
                StopSound(handle, fadeOutTime);
            }
        }

        public void SetGlobalVolume(float volume)
        {
            _globalVolume = Mathf.Clamp01(volume);

            // Update all active sounds
            foreach (var handle in _activeSounds)
            {
                if (handle.audioSource != null)
                {
                    // Would need to store original volume to properly update this
                    // This is a simplified version
                    handle.audioSource.volume *= _globalVolume;
                }
            }
        }

        public void PauseAll()
        {
            _isPaused = true;
            foreach (var handle in _activeSounds)
            {
                if (handle.audioSource != null && handle.audioSource.isPlaying)
                {
                    handle.audioSource.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            _isPaused = false;
            foreach (var handle in _activeSounds)
            {
                if (handle.audioSource != null)
                {
                    handle.audioSource.UnPause();
                }
            }
        }

        public bool IsPlaying(string soundId)
        {
            foreach (var handle in _activeSounds)
            {
                if (handle.soundId == soundId && handle.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all available sound IDs that can be played
        /// </summary>
        /// <returns>Array of available sound IDs</returns>
        public string[] GetAvailableSoundIds()
        {
            var soundIds = new string[_soundDefinitions.Count];
            _soundDefinitions.Keys.CopyTo(soundIds, 0);
            return soundIds;
        }

        private IEnumerator FadeIn(AudioSource audioSource, float duration, float targetVolume)
        {
            float startVolume = 0f;
            audioSource.volume = startVolume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            audioSource.volume = targetVolume;
        }

        private IEnumerator FadeOutAndStop(SoundHandle handle, float duration)
        {
            AudioSource audioSource = handle.audioSource;
            if (audioSource == null) yield break;

            float startVolume = audioSource.volume;
            float elapsed = 0f;

            while (elapsed < duration && audioSource != null)
            {
                elapsed += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                SoundEvents.InvokeSoundStopped(handle.soundId, handle);
                CompleteSoundHandle(handle);
            }
        }

        private void Update()
        {
            // Clean up completed sounds that might have been missed
            for (int i = _activeSounds.Count - 1; i >= 0; i--)
            {
                var handle = _activeSounds[i];
                if (handle.audioSource == null || (!handle.audioSource.isPlaying && !handle.audioSource.loop))
                {
                    CompleteSoundHandle(handle);
                }
            }
        }

        private void OnDestroy()
        {
            StopAllSounds();
        }
    }
}