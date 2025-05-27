using System;
using UnityEngine;

namespace Ludo.AudioFlux.Modules
{
    /// <summary>
    /// Base class for AudioFlux modules providing common functionality
    /// </summary>
    public abstract class AudioFluxModuleBase : IAudioFluxModule
    {
        protected IMusicService MusicService { get; private set; }
        protected ISFXService SfxService { get; private set; }
        
        private bool _isInitialized = false;
        private bool _isEnabled = false;
        
        public abstract string ModuleId { get; }
        public abstract string ModuleName { get; }
        public abstract Version ModuleVersion { get; }
        public virtual string[] Dependencies => new string[0];
        
        public bool IsEnabled => _isEnabled;
        
        public virtual void Initialize(IMusicService musicService, ISFXService sfxService)
        {
            if (_isInitialized)
                return;
            
            MusicService = musicService ?? throw new ArgumentNullException(nameof(musicService));
            SfxService = sfxService ?? throw new ArgumentNullException(nameof(sfxService));
            
            OnInitialize();
            _isInitialized = true;
        }
        
        public virtual void OnModuleEnabled()
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"Module {ModuleId} must be initialized before enabling");
            
            if (_isEnabled)
                return;
            
            OnEnable();
            _isEnabled = true;
        }
        
        public virtual void OnModuleDisabled()
        {
            if (!_isEnabled)
                return;
            
            OnDisable();
            _isEnabled = false;
        }
        
        public virtual void OnModuleDestroy()
        {
            if (_isEnabled)
            {
                OnModuleDisabled();
            }
            
            OnDestroy();
            _isInitialized = false;
        }
        
        public virtual void OnModuleUpdate()
        {
            if (_isEnabled)
            {
                OnUpdate();
            }
        }
        
        public virtual bool IsCompatible(Version audioFluxVersion)
        {
            // Default implementation - modules are compatible with any version
            // Override this to implement specific version requirements
            return true;
        }
        
        /// <summary>
        /// Called during module initialization
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// Called when the module is enabled
        /// </summary>
        protected virtual void OnEnable() { }
        
        /// <summary>
        /// Called when the module is disabled
        /// </summary>
        protected virtual void OnDisable() { }
        
        /// <summary>
        /// Called when the module is destroyed
        /// </summary>
        protected virtual void OnDestroy() { }
        
        /// <summary>
        /// Called each frame when the module is enabled
        /// </summary>
        protected virtual void OnUpdate() { }
        
        /// <summary>
        /// Log a message with the module prefix
        /// </summary>
        protected void LogInfo(string message)
        {
            Debug.Log($"[{ModuleId}] {message}");
        }
        
        /// <summary>
        /// Log a warning with the module prefix
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{ModuleId}] {message}");
        }
        
        /// <summary>
        /// Log an error with the module prefix
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{ModuleId}] {message}");
        }
    }
    
    /// <summary>
    /// Base class for modules that extend the Music Service
    /// </summary>
    public abstract class MusicServiceModuleBase : AudioFluxModuleBase, IMusicServiceModule
    {
        public virtual void OnMusicStarted(string musicId, MusicHandle handle)
        {
            if (IsEnabled)
            {
                OnMusicStartedInternal(musicId, handle);
            }
        }
        
        public virtual void OnMusicStopped(string musicId, MusicHandle handle)
        {
            if (IsEnabled)
            {
                OnMusicStoppedInternal(musicId, handle);
            }
        }
        
        public virtual void OnCrossfadeStarted(string fromId, string toId, float duration)
        {
            if (IsEnabled)
            {
                OnCrossfadeStartedInternal(fromId, toId, duration);
            }
        }
        
        public virtual void OnBeat(int beat)
        {
            if (IsEnabled)
            {
                OnBeatInternal(beat);
            }
        }
        
        public virtual void OnBar(int bar)
        {
            if (IsEnabled)
            {
                OnBarInternal(bar);
            }
        }
        
        /// <summary>
        /// Called when music starts playing (only when module is enabled)
        /// </summary>
        protected virtual void OnMusicStartedInternal(string musicId, MusicHandle handle) { }
        
        /// <summary>
        /// Called when music stops playing (only when module is enabled)
        /// </summary>
        protected virtual void OnMusicStoppedInternal(string musicId, MusicHandle handle) { }
        
        /// <summary>
        /// Called when a crossfade begins (only when module is enabled)
        /// </summary>
        protected virtual void OnCrossfadeStartedInternal(string fromId, string toId, float duration) { }
        
        /// <summary>
        /// Called on each beat (only when module is enabled)
        /// </summary>
        protected virtual void OnBeatInternal(int beat) { }
        
        /// <summary>
        /// Called on each bar (only when module is enabled)
        /// </summary>
        protected virtual void OnBarInternal(int bar) { }
    }
    
    /// <summary>
    /// Base class for modules that extend the SFX Service
    /// </summary>
    public abstract class SFXServiceModuleBase : AudioFluxModuleBase, ISFXServiceModule
    {
        public virtual void OnSoundStarted(string soundId, SoundHandle handle)
        {
            if (IsEnabled)
            {
                OnSoundStartedInternal(soundId, handle);
            }
        }
        
        public virtual void OnSoundStopped(string soundId, SoundHandle handle)
        {
            if (IsEnabled)
            {
                OnSoundStoppedInternal(soundId, handle);
            }
        }
        
        public virtual void OnSoundCompleted(string soundId, SoundHandle handle)
        {
            if (IsEnabled)
            {
                OnSoundCompletedInternal(soundId, handle);
            }
        }
        
        /// <summary>
        /// Called when a sound starts playing (only when module is enabled)
        /// </summary>
        protected virtual void OnSoundStartedInternal(string soundId, SoundHandle handle) { }
        
        /// <summary>
        /// Called when a sound stops playing (only when module is enabled)
        /// </summary>
        protected virtual void OnSoundStoppedInternal(string soundId, SoundHandle handle) { }
        
        /// <summary>
        /// Called when a sound completes playing (only when module is enabled)
        /// </summary>
        protected virtual void OnSoundCompletedInternal(string soundId, SoundHandle handle) { }
    }
    
    /// <summary>
    /// Base class for modules that extend both Music and SFX services
    /// </summary>
    public abstract class HybridServiceModuleBase : AudioFluxModuleBase, IMusicServiceModule, ISFXServiceModule
    {
        // Music Service Events
        public virtual void OnMusicStarted(string musicId, MusicHandle handle)
        {
            if (IsEnabled) OnMusicStartedInternal(musicId, handle);
        }
        
        public virtual void OnMusicStopped(string musicId, MusicHandle handle)
        {
            if (IsEnabled) OnMusicStoppedInternal(musicId, handle);
        }
        
        public virtual void OnCrossfadeStarted(string fromId, string toId, float duration)
        {
            if (IsEnabled) OnCrossfadeStartedInternal(fromId, toId, duration);
        }
        
        public virtual void OnBeat(int beat)
        {
            if (IsEnabled) OnBeatInternal(beat);
        }
        
        public virtual void OnBar(int bar)
        {
            if (IsEnabled) OnBarInternal(bar);
        }
        
        // SFX Service Events
        public virtual void OnSoundStarted(string soundId, SoundHandle handle)
        {
            if (IsEnabled) OnSoundStartedInternal(soundId, handle);
        }
        
        public virtual void OnSoundStopped(string soundId, SoundHandle handle)
        {
            if (IsEnabled) OnSoundStoppedInternal(soundId, handle);
        }
        
        public virtual void OnSoundCompleted(string soundId, SoundHandle handle)
        {
            if (IsEnabled) OnSoundCompletedInternal(soundId, handle);
        }
        
        // Protected virtual methods for derived classes to override
        protected virtual void OnMusicStartedInternal(string musicId, MusicHandle handle) { }
        protected virtual void OnMusicStoppedInternal(string musicId, MusicHandle handle) { }
        protected virtual void OnCrossfadeStartedInternal(string fromId, string toId, float duration) { }
        protected virtual void OnBeatInternal(int beat) { }
        protected virtual void OnBarInternal(int bar) { }
        protected virtual void OnSoundStartedInternal(string soundId, SoundHandle handle) { }
        protected virtual void OnSoundStoppedInternal(string soundId, SoundHandle handle) { }
        protected virtual void OnSoundCompletedInternal(string soundId, SoundHandle handle) { }
    }
}
