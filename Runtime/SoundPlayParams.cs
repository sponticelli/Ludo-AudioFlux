using System;
using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Sound playback parameters
    /// </summary>
    public struct SoundPlayParams
    {
        public Vector3? position;
        public Transform followTarget;
        public float? volumeMultiplier;
        public float? pitchMultiplier;
        public bool? loop;
        public Action<SoundHandle> onComplete;
        
        public static SoundPlayParams Default => new SoundPlayParams();
        
        public static SoundPlayParams At(Vector3 position) => new SoundPlayParams { position = position };
        public static SoundPlayParams Following(Transform target) => new SoundPlayParams { followTarget = target };
        public static SoundPlayParams WithVolume(float volume) => new SoundPlayParams { volumeMultiplier = volume };
    }
}