using System;

namespace Ludo.AudioFlux.Modules
{
    /// <summary>
    /// Events related to module lifecycle and management
    /// </summary>
    public static class ModuleEvents
    {
        /// <summary>
        /// Fired when a module is discovered
        /// </summary>
        public static event Action<IAudioFluxModule> OnModuleDiscovered;
        
        /// <summary>
        /// Fired when a module is successfully initialized
        /// </summary>
        public static event Action<IAudioFluxModule> OnModuleInitialized;
        
        /// <summary>
        /// Fired when a module is enabled
        /// </summary>
        public static event Action<IAudioFluxModule> OnModuleEnabled;
        
        /// <summary>
        /// Fired when a module is disabled
        /// </summary>
        public static event Action<IAudioFluxModule> OnModuleDisabled;
        
        /// <summary>
        /// Fired when a module fails to initialize
        /// </summary>
        public static event Action<IAudioFluxModule, Exception> OnModuleInitializationFailed;
        
        /// <summary>
        /// Fired when a module is destroyed
        /// </summary>
        public static event Action<IAudioFluxModule> OnModuleDestroyed;
        
        /// <summary>
        /// Fired when module dependencies are resolved
        /// </summary>
        public static event Action<IAudioFluxModule, string[]> OnModuleDependenciesResolved;
        
        /// <summary>
        /// Fired when module dependencies fail to resolve
        /// </summary>
        public static event Action<IAudioFluxModule, string[]> OnModuleDependenciesFailed;
        
        // Internal methods for invoking events
        internal static void InvokeModuleDiscovered(IAudioFluxModule module) => OnModuleDiscovered?.Invoke(module);
        internal static void InvokeModuleInitialized(IAudioFluxModule module) => OnModuleInitialized?.Invoke(module);
        internal static void InvokeModuleEnabled(IAudioFluxModule module) => OnModuleEnabled?.Invoke(module);
        internal static void InvokeModuleDisabled(IAudioFluxModule module) => OnModuleDisabled?.Invoke(module);
        internal static void InvokeModuleInitializationFailed(IAudioFluxModule module, Exception exception) => OnModuleInitializationFailed?.Invoke(module, exception);
        internal static void InvokeModuleDestroyed(IAudioFluxModule module) => OnModuleDestroyed?.Invoke(module);
        internal static void InvokeModuleDependenciesResolved(IAudioFluxModule module, string[] dependencies) => OnModuleDependenciesResolved?.Invoke(module, dependencies);
        internal static void InvokeModuleDependenciesFailed(IAudioFluxModule module, string[] dependencies) => OnModuleDependenciesFailed?.Invoke(module, dependencies);
    }
    
    /// <summary>
    /// Events for advanced audio features that modules can listen to
    /// </summary>
    public static class AdvancedAudioEvents
    {
        /// <summary>
        /// Fired when audio occlusion should be calculated
        /// </summary>
        public static event Action<UnityEngine.Vector3, UnityEngine.Vector3, SoundHandle> OnAudioOcclusionRequested;
        
        /// <summary>
        /// Fired when reverb zone changes are detected
        /// </summary>
        public static event Action<UnityEngine.AudioReverbZone, SoundHandle> OnReverbZoneChanged;
        
        /// <summary>
        /// Fired when surface material detection is needed
        /// </summary>
        public static event Action<UnityEngine.Vector3, UnityEngine.RaycastHit> OnSurfaceMaterialDetected;
        
        /// <summary>
        /// Fired when collision audio should be generated
        /// </summary>
        public static event Action<UnityEngine.Collision, float> OnCollisionAudioRequested;
        
        /// <summary>
        /// Fired when game state changes for dynamic music
        /// </summary>
        public static event Action<string, object> OnGameStateChanged;
        
        /// <summary>
        /// Fired when music intensity should change
        /// </summary>
        public static event Action<float> OnMusicIntensityChanged;
        
        // Internal methods for invoking events
        internal static void InvokeAudioOcclusionRequested(UnityEngine.Vector3 source, UnityEngine.Vector3 listener, SoundHandle handle) => OnAudioOcclusionRequested?.Invoke(source, listener, handle);
        internal static void InvokeReverbZoneChanged(UnityEngine.AudioReverbZone reverbZone, SoundHandle handle) => OnReverbZoneChanged?.Invoke(reverbZone, handle);
        internal static void InvokeSurfaceMaterialDetected(UnityEngine.Vector3 position, UnityEngine.RaycastHit hit) => OnSurfaceMaterialDetected?.Invoke(position, hit);
        internal static void InvokeCollisionAudioRequested(UnityEngine.Collision collision, float intensity) => OnCollisionAudioRequested?.Invoke(collision, intensity);
        internal static void InvokeGameStateChanged(string stateName, object stateData) => OnGameStateChanged?.Invoke(stateName, stateData);
        internal static void InvokeMusicIntensityChanged(float intensity) => OnMusicIntensityChanged?.Invoke(intensity);
    }
}
