using System;
using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Sound instance handle for controlling playback
    /// </summary>
    public class SoundHandle
    {
        public string soundId { get; internal set; }
        public AudioSource audioSource { get; internal set; }
        public bool isValid => audioSource != null && audioSource.isPlaying;
        public float time => isValid ? audioSource.time : 0f;
        public bool isPlaying => isValid;
        
        internal Action<SoundHandle> onComplete;
        internal Coroutine fadeCoroutine;
    }
}