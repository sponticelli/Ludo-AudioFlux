using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Ludo.AudioFlux
{
    public class MusicService : MonoBehaviour, IMusicService
    {
        [Header("Music Library")]
        [SerializeField] private MusicDefinition[] musicLibrary;

        [Header("Audio Sources")]
        [SerializeField] private int maxConcurrentMusic = 2;

        [Header("Crossfade Settings")]
        [SerializeField] private float defaultCrossfadeDuration = 1f;

        [Header("Beat Tracking")]
        [SerializeField] private bool enableBeatTracking = true;

        private readonly Dictionary<string, MusicDefinition> _musicDefinitions = new Dictionary<string, MusicDefinition>();
        private readonly List<AudioSource> _audioSources = new List<AudioSource>();
        private readonly List<MusicHandle> _activeMusic = new List<MusicHandle>();
        private readonly Dictionary<string, AudioClip> _preloadedClips = new Dictionary<string, AudioClip>();

        private MusicHandle _currentMusic;
        private float _globalVolume = 1f;
        private bool _isPaused;
        private float _duckingLevel = 1f;
        private Coroutine _duckingCoroutine;

        private void Awake()
        {
            InitializeAudioSources();
            RegisterMusicLibrary();
        }

        private void InitializeAudioSources()
        {
            for (int i = 0; i < maxConcurrentMusic; i++)
            {
                CreateAudioSource();
            }
        }

        private void RegisterMusicLibrary()
        {
            foreach (var musicDef in musicLibrary)
            {
                if (musicDef != null)
                {
                    RegisterMusic(musicDef.name, musicDef);
                }
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject audioObj = new GameObject($"MusicAudioSource_{_audioSources.Count}");
            audioObj.transform.SetParent(transform);
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            _audioSources.Add(audioSource);
            return audioSource;
        }

        private AudioSource GetAvailableAudioSource()
        {
            // Find an unused audio source
            foreach (var source in _audioSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // If all are in use, create a new one if we haven't reached the limit
            if (_audioSources.Count < maxConcurrentMusic)
            {
                return CreateAudioSource();
            }

            // Return the oldest playing source (will be stopped)
            return _audioSources[0];
        }

        public MusicHandle PlayMusic(string musicId, MusicPlayParams playParams = default)
        {
            if (!_musicDefinitions.TryGetValue(musicId, out var musicDef))
            {
                Debug.LogWarning($"Music '{musicId}' not found in library");
                return null;
            }

            return PlayMusic(musicDef, playParams);
        }

        public MusicHandle PlayMusic(MusicDefinition musicDef, MusicPlayParams playParams = default)
        {
            if (musicDef?.MusicClip == null)
            {
                Debug.LogWarning("MusicDefinition or MusicClip is null");
                return null;
            }

            // Stop current music if playing
            if (_currentMusic != null && _currentMusic.isPlaying)
            {
                StopMusic(_currentMusic, musicDef.FadeOutDuration);
            }

            AudioSource mainSource = GetAvailableAudioSource();
            AudioSource introSource = null;
            List<AudioSource> layerSources = new List<AudioSource>();

            // Setup intro source if needed
            if (musicDef.HasIntro && (playParams.playIntro ?? true))
            {
                introSource = GetAvailableAudioSource();
                ConfigureAudioSource(introSource, musicDef, playParams, true);
            }

            // Setup main source
            ConfigureAudioSource(mainSource, musicDef, playParams, false);

            // Setup layer sources if needed
            if (musicDef.HasLayers && (playParams.enableLayers ?? true))
            {
                for (int i = 0; i < musicDef.Layers.Length; i++)
                {
                    if (musicDef.Layers[i] != null)
                    {
                        AudioSource layerSource = GetAvailableAudioSource();
                        ConfigureLayerSource(layerSource, musicDef, i, playParams);
                        layerSources.Add(layerSource);
                    }
                }
            }

            // Create music handle
            MusicHandle handle = new MusicHandle
            {
                musicId = musicDef.name,
                definition = musicDef,
                mainSource = mainSource,
                introSource = introSource,
                layerSources = layerSources,
                onComplete = playParams.onComplete,
                onIntroComplete = playParams.onIntroComplete,
                onBeat = playParams.onBeat,
                onBar = playParams.onBar,
                baseVolume = musicDef.Volume * (playParams.volumeMultiplier ?? 1f) * _globalVolume
            };

            // Start playback
            if (introSource != null)
            {
                introSource.Play();
                StartCoroutine(HandleIntroCompletion(handle));
            }
            else
            {
                mainSource.Play();
                foreach (var layerSource in layerSources)
                {
                    layerSource.Play();
                }
            }

            // Start beat tracking if enabled
            if (enableBeatTracking && (playParams.onBeat != null || playParams.onBar != null))
            {
                handle.beatCoroutine = StartCoroutine(TrackBeats(handle));
            }

            // Apply fade in
            if (musicDef.FadeInDuration > 0)
            {
                handle.fadeCoroutine = StartCoroutine(FadeIn(handle, musicDef.FadeInDuration));
            }

            _activeMusic.Add(handle);
            _currentMusic = handle;

            return handle;
        }

        private void ConfigureAudioSource(AudioSource source, MusicDefinition musicDef, MusicPlayParams playParams, bool isIntro)
        {
            source.clip = isIntro ? musicDef.IntroClip : musicDef.MusicClip;
            source.volume = 0f; // Will be set by fade in or immediately
            source.pitch = musicDef.Pitch * (playParams.pitchMultiplier ?? 1f);
            source.loop = isIntro ? false : (playParams.loop ?? musicDef.Loop);
            source.outputAudioMixerGroup = musicDef.MixerGroup;

            if (playParams.startTime.HasValue && !isIntro)
            {
                source.time = playParams.startTime.Value;
            }
        }

        private void ConfigureLayerSource(AudioSource source, MusicDefinition musicDef, int layerIndex, MusicPlayParams playParams)
        {
            source.clip = musicDef.Layers[layerIndex];
            source.volume = 0f; // Will be set based on layer volumes
            source.pitch = musicDef.Pitch * (playParams.pitchMultiplier ?? 1f);
            source.loop = musicDef.Loop;
            source.outputAudioMixerGroup = musicDef.MixerGroup;

            if (playParams.startTime.HasValue)
            {
                source.time = playParams.startTime.Value;
            }
        }

        private IEnumerator HandleIntroCompletion(MusicHandle handle)
        {
            yield return new WaitWhile(() => handle.introSource.isPlaying);

            // Start main music and layers
            handle.mainSource.Play();
            foreach (var layerSource in handle.layerSources)
            {
                layerSource.Play();
            }

            // Invoke intro completion callback
            handle.onIntroComplete?.Invoke(handle);
        }

        private IEnumerator TrackBeats(MusicHandle handle)
        {
            int currentBeat = 0;
            int currentBar = 0;
            float beatDuration = handle.definition.BeatDuration;

            while (handle.isPlaying)
            {
                yield return new WaitForSeconds(beatDuration);

                if (handle.isPlaying)
                {
                    currentBeat++;
                    handle.onBeat?.Invoke(currentBeat);

                    if (currentBeat % handle.definition.BeatsPerBar == 0)
                    {
                        currentBar++;
                        handle.onBar?.Invoke(currentBar);
                    }
                }
            }
        }

        private IEnumerator FadeIn(MusicHandle handle, float duration)
        {
            float elapsed = 0f;
            float targetVolume = handle.GetEffectiveVolume();

            while (elapsed < duration && handle.isValid)
            {
                elapsed += Time.deltaTime;
                float volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);

                if (handle.mainSource != null)
                    handle.mainSource.volume = volume;

                if (handle.introSource != null)
                    handle.introSource.volume = volume;

                // Apply layer volumes
                for (int i = 0; i < handle.layerSources.Count; i++)
                {
                    if (handle.layerSources[i] != null)
                    {
                        float layerVolume = volume;
                        if (handle.definition.LayerVolumes != null && i < handle.definition.LayerVolumes.Length)
                        {
                            layerVolume *= handle.definition.LayerVolumes[i];
                        }
                        handle.layerSources[i].volume = layerVolume;
                    }
                }

                yield return null;
            }
        }

        // Additional methods will be added in the next part...

        public void StopMusic(float fadeOutTime = 0f)
        {
            if (_currentMusic != null)
            {
                StopMusic(_currentMusic, fadeOutTime);
            }
        }

        public void StopMusic(MusicHandle handle, float fadeOutTime = 0f)
        {
            if (handle == null || !handle.isValid) return;

            if (fadeOutTime > 0f)
            {
                if (handle.fadeCoroutine != null)
                    StopCoroutine(handle.fadeCoroutine);
                handle.fadeCoroutine = StartCoroutine(FadeOut(handle, fadeOutTime));
            }
            else
            {
                StopMusicImmediate(handle);
            }
        }

        private IEnumerator FadeOut(MusicHandle handle, float duration)
        {
            float elapsed = 0f;
            float startVolume = handle.GetEffectiveVolume();

            while (elapsed < duration && handle.isValid)
            {
                elapsed += Time.deltaTime;
                float volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);

                if (handle.mainSource != null)
                    handle.mainSource.volume = volume;

                if (handle.introSource != null)
                    handle.introSource.volume = volume;

                foreach (var layerSource in handle.layerSources)
                {
                    if (layerSource != null)
                        layerSource.volume = volume;
                }

                yield return null;
            }

            StopMusicImmediate(handle);
        }

        private void StopMusicImmediate(MusicHandle handle)
        {
            if (handle.mainSource != null)
                handle.mainSource.Stop();

            if (handle.introSource != null)
                handle.introSource.Stop();

            foreach (var layerSource in handle.layerSources)
            {
                if (layerSource != null)
                    layerSource.Stop();
            }

            if (handle.beatCoroutine != null)
                StopCoroutine(handle.beatCoroutine);

            if (handle.fadeCoroutine != null)
                StopCoroutine(handle.fadeCoroutine);

            _activeMusic.Remove(handle);

            if (_currentMusic == handle)
                _currentMusic = null;

            handle.onComplete?.Invoke(handle);
        }

        public void PauseMusic()
        {
            _isPaused = true;
            foreach (var handle in _activeMusic)
            {
                if (handle.mainSource != null && handle.mainSource.isPlaying)
                    handle.mainSource.Pause();

                if (handle.introSource != null && handle.introSource.isPlaying)
                    handle.introSource.Pause();

                foreach (var layerSource in handle.layerSources)
                {
                    if (layerSource != null && layerSource.isPlaying)
                        layerSource.Pause();
                }
            }
        }

        public void ResumeMusic()
        {
            _isPaused = false;
            foreach (var handle in _activeMusic)
            {
                if (handle.mainSource != null)
                    handle.mainSource.UnPause();

                if (handle.introSource != null)
                    handle.introSource.UnPause();

                foreach (var layerSource in handle.layerSources)
                {
                    if (layerSource != null)
                        layerSource.UnPause();
                }
            }
        }

        public void SetMusicVolume(float volume)
        {
            _globalVolume = Mathf.Clamp01(volume);

            foreach (var handle in _activeMusic)
            {
                UpdateHandleVolume(handle);
            }
        }

        private void UpdateHandleVolume(MusicHandle handle)
        {
            float effectiveVolume = handle.GetEffectiveVolume();

            if (handle.mainSource != null)
                handle.mainSource.volume = effectiveVolume;

            if (handle.introSource != null)
                handle.introSource.volume = effectiveVolume;

            for (int i = 0; i < handle.layerSources.Count; i++)
            {
                if (handle.layerSources[i] != null)
                {
                    float layerVolume = effectiveVolume;
                    if (handle.definition.LayerVolumes != null && i < handle.definition.LayerVolumes.Length)
                    {
                        layerVolume *= handle.definition.LayerVolumes[i];
                    }
                    handle.layerSources[i].volume = layerVolume;
                }
            }
        }

        public void CrossfadeTo(string musicId, float crossfadeDuration = 1f, MusicPlayParams playParams = default)
        {
            if (!_musicDefinitions.TryGetValue(musicId, out var musicDef))
            {
                Debug.LogWarning($"Music '{musicId}' not found in library");
                return;
            }

            CrossfadeTo(musicDef, crossfadeDuration, playParams);
        }

        public void CrossfadeTo(MusicDefinition musicDef, float crossfadeDuration = 1f, MusicPlayParams playParams = default)
        {
            if (crossfadeDuration <= 0f)
            {
                crossfadeDuration = defaultCrossfadeDuration;
            }

            MusicHandle oldMusic = _currentMusic;
            MusicHandle newMusic = PlayMusic(musicDef, playParams);

            if (oldMusic != null && newMusic != null)
            {
                StartCoroutine(PerformCrossfade(oldMusic, newMusic, crossfadeDuration));
            }
        }

        private IEnumerator PerformCrossfade(MusicHandle oldMusic, MusicHandle newMusic, float duration)
        {
            float elapsed = 0f;
            float oldStartVolume = oldMusic.GetEffectiveVolume();
            float newTargetVolume = newMusic.GetEffectiveVolume();

            // Start new music at 0 volume
            if (newMusic.mainSource != null)
                newMusic.mainSource.volume = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Fade out old music
                float oldVolume = Mathf.Lerp(oldStartVolume, 0f, t);
                if (oldMusic.mainSource != null)
                    oldMusic.mainSource.volume = oldVolume;

                // Fade in new music
                float newVolume = Mathf.Lerp(0f, newTargetVolume, t);
                if (newMusic.mainSource != null)
                    newMusic.mainSource.volume = newVolume;

                yield return null;
            }

            // Stop old music
            StopMusicImmediate(oldMusic);
        }

        public bool IsPlaying()
        {
            return _currentMusic != null && _currentMusic.isPlaying;
        }

        public bool IsPlaying(string musicId)
        {
            return _activeMusic.Any(handle => handle.musicId == musicId && handle.isPlaying);
        }

        public MusicHandle GetCurrentMusic()
        {
            return _currentMusic;
        }

        public void RegisterMusic(string id, MusicDefinition musicDefinition)
        {
            if (musicDefinition != null)
            {
                _musicDefinitions[id] = musicDefinition;
            }
        }

        public void PreloadMusic(string musicId)
        {
            if (_musicDefinitions.TryGetValue(musicId, out var musicDef))
            {
                if (musicDef.MusicClip != null)
                    _preloadedClips[musicId + "_main"] = musicDef.MusicClip;

                if (musicDef.IntroClip != null)
                    _preloadedClips[musicId + "_intro"] = musicDef.IntroClip;

                if (musicDef.Layers != null)
                {
                    for (int i = 0; i < musicDef.Layers.Length; i++)
                    {
                        if (musicDef.Layers[i] != null)
                            _preloadedClips[musicId + "_layer_" + i] = musicDef.Layers[i];
                    }
                }
            }
        }

        public string[] GetAvailableMusicIds()
        {
            return _musicDefinitions.Keys.ToArray();
        }

        public void SetDucking(float duckingLevel, float duckingTime = 0.5f)
        {
            if (_duckingCoroutine != null)
                StopCoroutine(_duckingCoroutine);

            _duckingCoroutine = StartCoroutine(ApplyDucking(duckingLevel, duckingTime));
        }

        private IEnumerator ApplyDucking(float targetLevel, float duration)
        {
            float startLevel = _duckingLevel;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _duckingLevel = Mathf.Lerp(startLevel, targetLevel, elapsed / duration);

                foreach (var handle in _activeMusic)
                {
                    handle.duckingMultiplier = _duckingLevel;
                    UpdateHandleVolume(handle);
                }

                yield return null;
            }

            _duckingLevel = targetLevel;
        }

        private void Update()
        {
            // Clean up finished music handles
            for (int i = _activeMusic.Count - 1; i >= 0; i--)
            {
                var handle = _activeMusic[i];
                if (!handle.isPlaying)
                {
                    StopMusicImmediate(handle);
                }
            }
        }
    }
}
