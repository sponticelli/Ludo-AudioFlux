using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Music instance handle for controlling playback
    /// </summary>
    public class MusicHandle
    {
        public string musicId { get; internal set; }
        public MusicDefinition definition { get; internal set; }
        public AudioSource mainSource { get; internal set; }
        public AudioSource introSource { get; internal set; }
        public List<AudioSource> layerSources { get; internal set; }
        
        public bool isValid => mainSource != null;
        public bool isPlaying => isValid && (mainSource.isPlaying || (introSource != null && introSource.isPlaying));
        
        public float time
        {
            get
            {
                if (!isValid) return 0f;
                
                if (introSource != null && introSource.isPlaying)
                    return introSource.time;
                else if (mainSource.isPlaying)
                {
                    float mainTime = mainSource.time;
                    if (definition.HasIntro && definition.IntroClip != null)
                        mainTime += definition.IntroClip.length;
                    return mainTime;
                }
                
                return 0f;
            }
        }
        
        public float normalizedTime
        {
            get
            {
                if (!isValid || definition == null) return 0f;
                float totalDuration = definition.TotalDuration;
                return totalDuration > 0 ? time / totalDuration : 0f;
            }
        }
        
        public int currentBeat
        {
            get
            {
                if (!isValid || definition == null) return 0;
                return Mathf.FloorToInt(time / definition.BeatDuration);
            }
        }
        
        public int currentBar
        {
            get
            {
                if (!isValid || definition == null) return 0;
                return Mathf.FloorToInt(time / definition.BarDuration);
            }
        }
        
        public float beatProgress
        {
            get
            {
                if (!isValid || definition == null) return 0f;
                return (time % definition.BeatDuration) / definition.BeatDuration;
            }
        }
        
        public float barProgress
        {
            get
            {
                if (!isValid || definition == null) return 0f;
                return (time % definition.BarDuration) / definition.BarDuration;
            }
        }
        
        internal Action<MusicHandle> onComplete;
        internal Action<MusicHandle> onIntroComplete;
        internal Action<int> onBeat;
        internal Action<int> onBar;
        internal Coroutine fadeCoroutine;
        internal Coroutine beatCoroutine;
        internal float baseVolume = 1f;
        internal float duckingMultiplier = 1f;
        
        /// <summary>
        /// Get the effective volume (including ducking)
        /// </summary>
        public float GetEffectiveVolume()
        {
            return baseVolume * duckingMultiplier;
        }
        
        /// <summary>
        /// Set layer volume (0-1)
        /// </summary>
        public void SetLayerVolume(int layerIndex, float volume)
        {
            if (layerSources == null || layerIndex < 0 || layerIndex >= layerSources.Count) return;
            var layerSource = layerSources[layerIndex];
            if (layerSource != null)
            {
                float layerBaseVolume = definition.LayerVolumes != null && layerIndex < definition.LayerVolumes.Length 
                    ? definition.LayerVolumes[layerIndex] 
                    : 1f;
                layerSource.volume = volume * layerBaseVolume * baseVolume * duckingMultiplier;
            }
        }
        
        /// <summary>
        /// Get layer volume (0-1)
        /// </summary>
        public float GetLayerVolume(int layerIndex)
        {
            if (layerSources == null || layerIndex < 0 || layerIndex >= layerSources.Count) return 0f;
            var layerSource = layerSources[layerIndex];
            if (layerSource != null)
            {
                float layerBaseVolume = definition.LayerVolumes != null && layerIndex < definition.LayerVolumes.Length 
                    ? definition.LayerVolumes[layerIndex] 
                    : 1f;
                return layerSource.volume / (layerBaseVolume * baseVolume * duckingMultiplier);
            }
            return 0f;
        }
    }
}