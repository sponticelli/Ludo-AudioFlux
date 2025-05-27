using System;
using UnityEngine;

namespace Ludo.AudioFlux.Modules
{
    /// <summary>
    /// Base interface for all AudioFlux modules
    /// </summary>
    public interface IAudioFluxModule
    {
        /// <summary>
        /// Unique identifier for this module
        /// </summary>
        string ModuleId { get; }
        
        /// <summary>
        /// Human-readable name for this module
        /// </summary>
        string ModuleName { get; }
        
        /// <summary>
        /// Version of this module
        /// </summary>
        Version ModuleVersion { get; }
        
        /// <summary>
        /// Whether this module is currently enabled
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Dependencies required by this module
        /// </summary>
        string[] Dependencies { get; }
        
        /// <summary>
        /// Initialize the module with the provided services
        /// </summary>
        /// <param name="musicService">Music service instance</param>
        /// <param name="sfxService">SFX service instance</param>
        void Initialize(IMusicService musicService, ISFXService sfxService);
        
        /// <summary>
        /// Called when the module should start its functionality
        /// </summary>
        void OnModuleEnabled();
        
        /// <summary>
        /// Called when the module should stop its functionality
        /// </summary>
        void OnModuleDisabled();
        
        /// <summary>
        /// Called when the module is being destroyed
        /// </summary>
        void OnModuleDestroy();
        
        /// <summary>
        /// Update method called each frame if the module needs it
        /// </summary>
        void OnModuleUpdate();
        
        /// <summary>
        /// Check if this module is compatible with the current AudioFlux version
        /// </summary>
        /// <param name="audioFluxVersion">Current AudioFlux version</param>
        /// <returns>True if compatible</returns>
        bool IsCompatible(Version audioFluxVersion);
    }
    
    /// <summary>
    /// Base interface for modules that extend the Music Service
    /// </summary>
    public interface IMusicServiceModule : IAudioFluxModule
    {
        /// <summary>
        /// Called when music starts playing
        /// </summary>
        /// <param name="musicId">ID of the music that started</param>
        /// <param name="handle">Handle to the playing music</param>
        void OnMusicStarted(string musicId, MusicHandle handle);
        
        /// <summary>
        /// Called when music stops playing
        /// </summary>
        /// <param name="musicId">ID of the music that stopped</param>
        /// <param name="handle">Handle to the stopped music</param>
        void OnMusicStopped(string musicId, MusicHandle handle);
        
        /// <summary>
        /// Called when a crossfade begins
        /// </summary>
        /// <param name="fromId">ID of the music being faded out</param>
        /// <param name="toId">ID of the music being faded in</param>
        /// <param name="duration">Duration of the crossfade</param>
        void OnCrossfadeStarted(string fromId, string toId, float duration);
        
        /// <summary>
        /// Called on each beat if beat tracking is enabled
        /// </summary>
        /// <param name="beat">Current beat number</param>
        void OnBeat(int beat);
        
        /// <summary>
        /// Called on each bar if beat tracking is enabled
        /// </summary>
        /// <param name="bar">Current bar number</param>
        void OnBar(int bar);
    }
    
    /// <summary>
    /// Base interface for modules that extend the SFX Service
    /// </summary>
    public interface ISFXServiceModule : IAudioFluxModule
    {
        /// <summary>
        /// Called when a sound starts playing
        /// </summary>
        /// <param name="soundId">ID of the sound that started</param>
        /// <param name="handle">Handle to the playing sound</param>
        void OnSoundStarted(string soundId, SoundHandle handle);
        
        /// <summary>
        /// Called when a sound stops playing
        /// </summary>
        /// <param name="soundId">ID of the sound that stopped</param>
        /// <param name="handle">Handle to the stopped sound</param>
        void OnSoundStopped(string soundId, SoundHandle handle);
        
        /// <summary>
        /// Called when a sound completes playing
        /// </summary>
        /// <param name="soundId">ID of the sound that completed</param>
        /// <param name="handle">Handle to the completed sound</param>
        void OnSoundCompleted(string soundId, SoundHandle handle);
    }
}
