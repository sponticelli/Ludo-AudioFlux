using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Example demonstrating how to use the MusicService
    /// </summary>
    public class MusicServiceExample : MonoBehaviour
    {
        [Header("Music Service")]
        [SerializeField] private MusicService musicService;
        
        [Header("Example Music IDs")]
        [SerializeField] private string backgroundMusicId = "background_music";
        [SerializeField] private string combatMusicId = "combat_music";
        [SerializeField] private string menuMusicId = "menu_music";
        
        [Header("Settings")]
        [SerializeField] private float crossfadeDuration = 2f;
        [SerializeField] private float duckingLevel = 0.3f;
        
        private MusicHandle currentMusicHandle;
        
        private void Start()
        {
            if (musicService == null)
            {
                musicService = FindObjectOfType<MusicService>();
            }
            
            if (musicService == null)
            {
                Debug.LogError("No MusicService found in scene!");
                return;
            }
            
            // Start with background music
            PlayBackgroundMusic();
        }
        
        [ContextMenu("Play Background Music")]
        public void PlayBackgroundMusic()
        {
            if (musicService == null) return;
            
            var playParams = new MusicPlayParamsBuilder()
                .WithVolume(0.8f)
                .WithLayers(true)
                .OnComplete(handle => Debug.Log($"Music {handle.musicId} completed"))
                .OnBeat(beat => Debug.Log($"Beat: {beat}"))
                .Build();
            
            currentMusicHandle = musicService.PlayMusic(backgroundMusicId, playParams);
        }
        
        [ContextMenu("Crossfade to Combat Music")]
        public void CrossfadeToCombatMusic()
        {
            if (musicService == null) return;
            
            var playParams = new MusicPlayParamsBuilder()
                .WithVolume(1f)
                .WithPitch(1.1f)
                .SkipIntro() // Skip intro for immediate combat feel
                .OnBeat(beat => 
                {
                    if (beat % 4 == 0) // Every bar
                    {
                        Debug.Log($"Combat beat: {beat}");
                    }
                })
                .Build();
            
            musicService.CrossfadeTo(combatMusicId, crossfadeDuration, playParams);
        }
        
        [ContextMenu("Crossfade to Menu Music")]
        public void CrossfadeToMenuMusic()
        {
            if (musicService == null) return;
            
            var playParams = MusicPlayParams.WithVolume(0.6f);
            musicService.CrossfadeTo(menuMusicId, crossfadeDuration, playParams);
        }
        
        [ContextMenu("Stop Music")]
        public void StopMusic()
        {
            if (musicService == null) return;
            
            musicService.StopMusic(1f); // Fade out over 1 second
        }
        
        [ContextMenu("Pause Music")]
        public void PauseMusic()
        {
            if (musicService == null) return;
            
            musicService.PauseMusic();
        }
        
        [ContextMenu("Resume Music")]
        public void ResumeMusic()
        {
            if (musicService == null) return;
            
            musicService.ResumeMusic();
        }
        
        [ContextMenu("Apply Ducking")]
        public void ApplyDucking()
        {
            if (musicService == null) return;
            
            // Duck the music (useful when dialogue or SFX needs to be prominent)
            musicService.SetDucking(duckingLevel, 0.5f);
        }
        
        [ContextMenu("Remove Ducking")]
        public void RemoveDucking()
        {
            if (musicService == null) return;
            
            // Return to normal volume
            musicService.SetDucking(1f, 0.5f);
        }
        
        [ContextMenu("Set Low Volume")]
        public void SetLowVolume()
        {
            if (musicService == null) return;
            
            musicService.SetMusicVolume(0.3f);
        }
        
        [ContextMenu("Set Normal Volume")]
        public void SetNormalVolume()
        {
            if (musicService == null) return;
            
            musicService.SetMusicVolume(1f);
        }
        
        [ContextMenu("Play Music with Custom Parameters")]
        public void PlayMusicWithCustomParameters()
        {
            if (musicService == null) return;
            
            // Example of complex music playback with all features
            var playParams = new MusicPlayParamsBuilder()
                .WithVolume(0.9f)
                .WithPitch(0.95f) // Slightly slower
                .WithLayers(true)
                .WithLayerVolumes(1f, 0.5f, 0.3f) // Different volumes for each layer
                .FromTime(30f) // Start 30 seconds into the track
                .OnComplete(handle => 
                {
                    Debug.Log($"Complex music playback of {handle.musicId} completed!");
                })
                .OnIntroComplete(handle => 
                {
                    Debug.Log($"Intro of {handle.musicId} completed, main loop starting");
                })
                .OnBeat(beat => 
                {
                    // Flash a light or trigger visual effects on beat
                    if (beat % 8 == 0) // Every 2 bars
                    {
                        Debug.Log($"Strong beat: {beat}");
                    }
                })
                .OnBar(bar => 
                {
                    Debug.Log($"Bar: {bar}");
                })
                .Build();
            
            currentMusicHandle = musicService.PlayMusic(backgroundMusicId, playParams);
        }
        
        [ContextMenu("Get Music Info")]
        public void GetMusicInfo()
        {
            if (musicService == null) return;
            
            Debug.Log($"Is any music playing: {musicService.IsPlaying()}");
            Debug.Log($"Is background music playing: {musicService.IsPlaying(backgroundMusicId)}");
            
            var currentMusic = musicService.GetCurrentMusic();
            if (currentMusic != null)
            {
                Debug.Log($"Current music: {currentMusic.musicId}");
                Debug.Log($"Has intro: {currentMusic.definition.HasIntro}");
                Debug.Log($"Has layers: {currentMusic.definition.HasLayers}");
                Debug.Log($"BPM: {currentMusic.definition.BPM}");
                Debug.Log($"Duration: {currentMusic.definition.TotalDuration}s");
            }
            else
            {
                Debug.Log("No music currently playing");
            }
            
            var availableMusic = musicService.GetAvailableMusicIds();
            Debug.Log($"Available music: {string.Join(", ", availableMusic)}");
        }
        
        [ContextMenu("Preload All Music")]
        public void PreloadAllMusic()
        {
            if (musicService == null) return;
            
            var availableMusic = musicService.GetAvailableMusicIds();
            foreach (var musicId in availableMusic)
            {
                musicService.PreloadMusic(musicId);
                Debug.Log($"Preloaded music: {musicId}");
            }
        }
        
        // Example of responding to game events
        public void OnPlayerEnteredCombat()
        {
            CrossfadeToCombatMusic();
            // Optionally apply ducking for combat sounds
            ApplyDucking();
        }
        
        public void OnPlayerExitedCombat()
        {
            CrossfadeToMenuMusic();
            RemoveDucking();
        }
        
        public void OnDialogueStarted()
        {
            // Duck music for dialogue
            ApplyDucking();
        }
        
        public void OnDialogueEnded()
        {
            // Return music to normal volume
            RemoveDucking();
        }
        
        public void OnGamePaused()
        {
            PauseMusic();
        }
        
        public void OnGameResumed()
        {
            ResumeMusic();
        }
    }
}
