using UnityEngine;
using UnityEngine.Audio;

namespace Ludo.AudioFlux
{
    
    /// <summary>
    /// Scriptable object for defining sound properties
    /// </summary>
    [CreateAssetMenu(fileName = "SoundDefinition", menuName = "Ludo/AudioFlux/SoundDefinition")]
    public class SoundDefinition : ScriptableObject
    {

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] audioClips;
        
        [Header("Playback Settings")]
        [SerializeField] private float pitch = 1f;
        [SerializeField] private float pitchVariance = 0f;
        [SerializeField] private float volume = 1f;
        [SerializeField] [Range(0f, 1f)] private float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        [SerializeField] private bool loop = false;
        
        [Header("Fade Settings")]
        [SerializeField] private  float fadeInDuration = 0f;
        [SerializeField] private  float fadeOutDuration = 0f;
        
        [Header("3D Audio Settings")]
        [SerializeField] [Range(0f, 500f)] private float maxDistance = 100f;
        [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        
        [Header("Priority")]
        [Range(0, 256)] public int priority = 128;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixerGroup mixerGroup;
        
        public AudioClip AudioClip => audioClips.Length == 0 ? null : audioClips[Random.Range(0, audioClips.Length)];
        public int ClipCount => audioClips.Length;
        
        public float Pitch => Random.Range(pitch - pitchVariance, pitch + pitchVariance);
        public float Volume => volume;
        public float SpatialBlend => spatialBlend;
        public bool Loop => loop;
        
        public float FadeInDuration => fadeInDuration;
        public float FadeOutDuration => fadeOutDuration;
        
        
        public float MaxDistance => maxDistance;
        public AudioRolloffMode RolloffMode => rolloffMode;
        
        public int Priority => priority;
        
        public AudioMixerGroup MixerGroup => mixerGroup;
    }
}