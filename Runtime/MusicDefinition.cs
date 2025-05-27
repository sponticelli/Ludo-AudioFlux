using UnityEngine;
using UnityEngine.Audio;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Scriptable object for defining music properties
    /// </summary>
    [CreateAssetMenu(fileName = "MusicDefinition", menuName = "Ludo/AudioFlux/MusicDefinition")]
    public class MusicDefinition : ScriptableObject
    {
        [Header("Audio Clips")]
        [Tooltip("Main music clip")]
        [SerializeField] private AudioClip musicClip;
        
        [Tooltip("Optional intro clip that plays before the main loop")]
        [SerializeField] private AudioClip introClip;
        
        [Header("Playback Settings")]
        [SerializeField] private float volume = 0.7f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private bool loop = true;
        
        [Header("Timing")]
        [Tooltip("BPM for beat-synced features")]
        [SerializeField] private float bpm = 120f;
        
        [Tooltip("Time signature (e.g., 4 for 4/4)")]
        [SerializeField] private int beatsPerBar = 4;
        
        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 1f;
        
        [Header("Layers")]
        [Tooltip("Additional layers that can be mixed in/out")]
        [SerializeField] private AudioClip[] layers;
        [SerializeField] private float[] layerVolumes;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixerGroup mixerGroup;
        
        [Header("Metadata")]
        [SerializeField] private string displayName;
        [SerializeField] private string artist;
        [SerializeField] private string album;
        [SerializeField] [TextArea(3, 5)] private string description;
        
        // Properties
        public AudioClip MusicClip => musicClip;
        public AudioClip IntroClip => introClip;
        public bool HasIntro => introClip != null;
        
        public float Volume => volume;
        public float Pitch => pitch;
        public bool Loop => loop;
        
        public float BPM => bpm;
        public int BeatsPerBar => beatsPerBar;
        public float BeatDuration => 60f / bpm;
        public float BarDuration => BeatDuration * beatsPerBar;
        
        public float FadeInDuration => fadeInDuration;
        public float FadeOutDuration => fadeOutDuration;
        
        public AudioClip[] Layers => layers;
        public float[] LayerVolumes => layerVolumes;
        public bool HasLayers => layers != null && layers.Length > 0;
        
        public AudioMixerGroup MixerGroup => mixerGroup;
        
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public string Artist => artist;
        public string Album => album;
        public string Description => description;
        
        /// <summary>
        /// Get the total duration including intro if present
        /// </summary>
        public float TotalDuration
        {
            get
            {
                float duration = 0f;
                if (HasIntro && introClip != null)
                    duration += introClip.length;
                if (musicClip != null)
                    duration += musicClip.length;
                return duration;
            }
        }
        
        /// <summary>
        /// Validates the music definition
        /// </summary>
        public bool IsValid()
        {
            if (musicClip == null)
            {
                Debug.LogError($"MusicDefinition '{name}' has no music clip assigned!");
                return false;
            }
            
            if (HasLayers && layerVolumes != null && layers.Length != layerVolumes.Length)
            {
                Debug.LogWarning($"MusicDefinition '{name}' has mismatched layer and volume counts!");
            }
            
            return true;
        }
    }
}