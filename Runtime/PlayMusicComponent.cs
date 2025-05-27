using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Component for easily playing music through the MusicService
    /// </summary>
    public class PlayMusicComponent : MonoBehaviour
    {
        [Header("Music Settings")]
        [SerializeField] private string musicId;
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private bool playOnEnable = false;
        
        [Header("Playback Parameters")]
        [SerializeField] private float volumeMultiplier = 1f;
        [SerializeField] private float pitchMultiplier = 1f;
        [SerializeField] private bool playIntro = true;
        [SerializeField] private bool enableLayers = true;
        [SerializeField] private float startTime = 0f;
        
        [Header("Crossfade Settings")]
        [SerializeField] private bool useCrossfade = false;
        [SerializeField] private float crossfadeDuration = 1f;
        
        [Header("Service Reference")]
        [SerializeField] private MusicService musicService;
        
        private MusicHandle _currentHandle;
        
        private void Start()
        {
            if (playOnStart)
            {
                PlayMusic();
            }
        }
        
        private void OnEnable()
        {
            if (playOnEnable && !playOnStart)
            {
                PlayMusic();
            }
        }
        
        private void OnDisable()
        {
            if (_currentHandle != null && _currentHandle.isPlaying)
            {
                StopMusic();
            }
        }
        
        /// <summary>
        /// Play the configured music
        /// </summary>
        public void PlayMusic()
        {
            if (string.IsNullOrEmpty(musicId))
            {
                Debug.LogWarning("Music ID is not set on " + gameObject.name);
                return;
            }
            
            MusicService service = GetMusicService();
            if (service == null)
            {
                Debug.LogWarning("No MusicService found for " + gameObject.name);
                return;
            }
            
            var playParams = CreatePlayParams();
            
            if (useCrossfade)
            {
                service.CrossfadeTo(musicId, crossfadeDuration, playParams);
            }
            else
            {
                _currentHandle = service.PlayMusic(musicId, playParams);
            }
        }
        
        /// <summary>
        /// Play specific music by ID
        /// </summary>
        public void PlayMusic(string id)
        {
            string originalId = musicId;
            musicId = id;
            PlayMusic();
            musicId = originalId;
        }
        
        /// <summary>
        /// Stop the current music
        /// </summary>
        public void StopMusic()
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                if (_currentHandle != null)
                {
                    service.StopMusic(_currentHandle);
                    _currentHandle = null;
                }
                else
                {
                    service.StopMusic();
                }
            }
        }
        
        /// <summary>
        /// Stop music with fade out
        /// </summary>
        public void StopMusic(float fadeOutTime)
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                if (_currentHandle != null)
                {
                    service.StopMusic(_currentHandle, fadeOutTime);
                    _currentHandle = null;
                }
                else
                {
                    service.StopMusic(fadeOutTime);
                }
            }
        }
        
        /// <summary>
        /// Crossfade to the configured music
        /// </summary>
        public void CrossfadeToMusic()
        {
            if (string.IsNullOrEmpty(musicId))
            {
                Debug.LogWarning("Music ID is not set on " + gameObject.name);
                return;
            }
            
            MusicService service = GetMusicService();
            if (service != null)
            {
                var playParams = CreatePlayParams();
                service.CrossfadeTo(musicId, crossfadeDuration, playParams);
            }
        }
        
        /// <summary>
        /// Crossfade to specific music by ID
        /// </summary>
        public void CrossfadeToMusic(string id)
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                var playParams = CreatePlayParams();
                service.CrossfadeTo(id, crossfadeDuration, playParams);
            }
        }
        
        /// <summary>
        /// Pause music
        /// </summary>
        public void PauseMusic()
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                service.PauseMusic();
            }
        }
        
        /// <summary>
        /// Resume music
        /// </summary>
        public void ResumeMusic()
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                service.ResumeMusic();
            }
        }
        
        /// <summary>
        /// Check if music is currently playing
        /// </summary>
        public bool IsPlaying()
        {
            MusicService service = GetMusicService();
            if (service != null)
            {
                if (!string.IsNullOrEmpty(musicId))
                {
                    return service.IsPlaying(musicId);
                }
                return service.IsPlaying();
            }
            return false;
        }
        
        private MusicService GetMusicService()
        {
            if (musicService != null)
                return musicService;
            
            // Try to find MusicService in the scene
            musicService = FindObjectOfType<MusicService>();
            return musicService;
        }
        
        private MusicPlayParams CreatePlayParams()
        {
            var builder = new MusicPlayParamsBuilder()
                .WithVolume(volumeMultiplier)
                .WithPitch(pitchMultiplier)
                .WithLayers(enableLayers);
            
            if (!playIntro)
                builder.SkipIntro();
            
            if (startTime > 0f)
                builder.FromTime(startTime);
            
            return builder.Build();
        }
        
        /// <summary>
        /// Set the music ID at runtime
        /// </summary>
        public void SetMusicId(string id)
        {
            musicId = id;
        }
        
        /// <summary>
        /// Get the current music ID
        /// </summary>
        public string GetMusicId()
        {
            return musicId;
        }
        
        /// <summary>
        /// Get the current music handle
        /// </summary>
        public MusicHandle GetCurrentHandle()
        {
            return _currentHandle;
        }
    }
}
