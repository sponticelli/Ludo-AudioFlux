using System;
using UnityEngine;
using Ludo.AudioFlux.Modules;

namespace Ludo.AudioFlux.Extensions
{
    /// <summary>
    /// Extension methods for AudioFlux services to support module functionality
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Play a sound with automatic module integration
        /// </summary>
        public static SoundHandle PlaySoundWithModules(this ISFXService sfxService, string soundId, SoundPlayParams playParams = default)
        {
            var handle = sfxService.PlaySound(soundId, playParams);

            // Trigger module events
            if (handle != null)
            {
                TriggerModuleEvents(handle, playParams);
            }

            return handle;
        }

        /// <summary>
        /// Play music with automatic module integration
        /// </summary>
        public static MusicHandle PlayMusicWithModules(this IMusicService musicService, string musicId, MusicPlayParams playParams = default)
        {
            var handle = musicService.PlayMusic(musicId, playParams);

            // Trigger module events
            if (handle != null)
            {
                TriggerMusicModuleEvents(handle, playParams);
            }

            return handle;
        }

        /// <summary>
        /// Request audio occlusion calculation for a sound
        /// </summary>
        public static void RequestOcclusion(this SoundHandle handle, Vector3? listenerPosition = null)
        {
            if (handle?.audioSource == null)
                return;

            var sourcePos = handle.audioSource.transform.position;
            var listenerPos = listenerPosition ?? GetListenerPosition();

            AdvancedAudioEvents.InvokeAudioOcclusionRequested(sourcePos, listenerPos, handle);
        }

        /// <summary>
        /// Set the game state for dynamic music modules
        /// </summary>
        public static void SetGameState(this IMusicService musicService, string stateName, object stateData = null)
        {
            AdvancedAudioEvents.InvokeGameStateChanged(stateName, stateData);
        }

        /// <summary>
        /// Set the music intensity for dynamic music modules
        /// </summary>
        public static void SetMusicIntensity(this IMusicService musicService, float intensity)
        {
            AdvancedAudioEvents.InvokeMusicIntensityChanged(Mathf.Clamp01(intensity));
        }

        /// <summary>
        /// Trigger surface material detection for physics-based audio
        /// </summary>
        public static void TriggerSurfaceMaterialDetection(this ISFXService sfxService, Vector3 position, RaycastHit hit)
        {
            AdvancedAudioEvents.InvokeSurfaceMaterialDetected(position, hit);
        }

        /// <summary>
        /// Request collision audio generation
        /// </summary>
        public static void RequestCollisionAudio(this ISFXService sfxService, Collision collision, float intensity = 1f)
        {
            AdvancedAudioEvents.InvokeCollisionAudioRequested(collision, intensity);
        }

        /// <summary>
        /// Check if a sound is currently occluded (requires occlusion module)
        /// </summary>
        public static bool IsOccluded(this SoundHandle handle)
        {
            var moduleManager = FindModuleManager();
            var occlusionModule = moduleManager?.GetModule<Modules.Advanced3D.AudioOcclusionModule>("advanced3d.occlusion");
            return occlusionModule?.IsOccluded(handle) ?? false;
        }

        /// <summary>
        /// Get the current LOD level of a sound (requires LOD module)
        /// </summary>
        public static Modules.Performance.AudioLODModule.AudioLODLevel GetLODLevel(this SoundHandle handle)
        {
            var moduleManager = FindModuleManager();
            var lodModule = moduleManager?.GetModule<Modules.Performance.AudioLODModule>("performance.audiolod");
            return lodModule?.GetLODLevel(handle) ?? Modules.Performance.AudioLODModule.AudioLODLevel.High;
        }

        /// <summary>
        /// Force a specific LOD level for a sound (requires LOD module)
        /// </summary>
        public static void ForceLODLevel(this SoundHandle handle, Modules.Performance.AudioLODModule.AudioLODLevel lodLevel)
        {
            var moduleManager = FindModuleManager();
            var lodModule = moduleManager?.GetModule<Modules.Performance.AudioLODModule>("performance.audiolod");
            lodModule?.ForceLODLevel(handle, lodLevel);
        }

        /// <summary>
        /// Mark a sound as important for better LOD treatment (requires LOD module)
        /// </summary>
        public static void SetImportant(this SoundHandle handle, bool isImportant = true)
        {
            var moduleManager = FindModuleManager();
            var lodModule = moduleManager?.GetModule<Modules.Performance.AudioLODModule>("performance.audiolod");
            lodModule?.SetSoundImportance(handle, isImportant);
        }

        /// <summary>
        /// Get the current music state (requires state-based music module)
        /// </summary>
        public static string GetCurrentMusicState(this IMusicService musicService)
        {
            var moduleManager = FindModuleManager();
            var stateModule = moduleManager?.GetModule<Modules.DynamicMusic.StateBasedMusicModule>("dynamic.statemusic");
            return stateModule?.GetCurrentState() ?? "unknown";
        }

        /// <summary>
        /// Transition to a specific music state (requires state-based music module)
        /// </summary>
        public static void TransitionToMusicState(this IMusicService musicService, string stateId, float? transitionTime = null)
        {
            var moduleManager = FindModuleManager();
            var stateModule = moduleManager?.GetModule<Modules.DynamicMusic.StateBasedMusicModule>("dynamic.statemusic");
            stateModule?.TransitionToState(stateId, transitionTime);
        }

        /// <summary>
        /// Get the current music intensity (requires state-based music module)
        /// </summary>
        public static float GetCurrentMusicIntensity(this IMusicService musicService)
        {
            var moduleManager = FindModuleManager();
            var stateModule = moduleManager?.GetModule<Modules.DynamicMusic.StateBasedMusicModule>("dynamic.statemusic");
            return stateModule?.GetCurrentIntensity() ?? 0.5f;
        }

        private static void TriggerModuleEvents(SoundHandle handle, SoundPlayParams playParams)
        {
            // Check if this is a positioned sound and trigger occlusion if needed
            if (handle?.audioSource != null &&
                handle.audioSource.spatialBlend > 0.5f &&
                (playParams.PositionMode == SoundPositionMode.AtPosition || playParams.followTarget != null))
            {
                var sourcePos = playParams.PositionMode == SoundPositionMode.AtPosition
                    ? playParams.Position
                    : handle.audioSource.transform.position;
                var listenerPos = GetListenerPosition();

                AdvancedAudioEvents.InvokeAudioOcclusionRequested(sourcePos, listenerPos, handle);
            }
        }

        private static void TriggerMusicModuleEvents(MusicHandle handle, MusicPlayParams playParams)
        {
            // Music-specific module events can be triggered here
            // For example, notifying dynamic music modules about new music starting
        }

        private static Vector3 GetListenerPosition()
        {
            var listener = UnityEngine.Object.FindObjectOfType<AudioListener>();
            if (listener != null)
            {
                return listener.transform.position;
            }

            var camera = Camera.main;
            if (camera != null)
            {
                return camera.transform.position;
            }

            return Vector3.zero;
        }

        private static AudioFluxModuleManager FindModuleManager()
        {
            return UnityEngine.Object.FindObjectOfType<AudioFluxModuleManager>();
        }
    }

    /// <summary>
    /// Builder pattern extensions for easier module integration
    /// </summary>
    public static class PlayParamsBuilderExtensions
    {
        /// <summary>
        /// Enable automatic occlusion for this sound
        /// </summary>
        public static SoundPlayParamsBuilder WithOcclusion(this SoundPlayParamsBuilder builder, bool enable = true)
        {
            // This would be implemented by storing a flag in the play params
            // and checking it in the module system
            return builder;
        }

        /// <summary>
        /// Set the importance level for LOD calculations
        /// </summary>
        public static SoundPlayParamsBuilder WithImportance(this SoundPlayParamsBuilder builder, bool isImportant = true)
        {
            // This would be implemented by storing importance in the play params
            return builder;
        }

        /// <summary>
        /// Force a specific LOD level for this sound
        /// </summary>
        public static SoundPlayParamsBuilder WithLODLevel(this SoundPlayParamsBuilder builder, Modules.Performance.AudioLODModule.AudioLODLevel lodLevel)
        {
            // This would be implemented by storing LOD level in the play params
            return builder;
        }
    }
}
