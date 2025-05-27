using UnityEngine;
using Ludo.AudioFlux;
using Ludo.AudioFlux.Modules;
using Ludo.AudioFlux.Extensions;

namespace Ludo.AudioFlux.Examples
{
    /// <summary>
    /// Example demonstrating how to use the AudioFlux module system
    /// </summary>
    public class ModuleSystemExample : MonoBehaviour
    {
        [Header("Service References")]
        [SerializeField] private MusicService musicService;
        [SerializeField] private SfxService sfxService;
        [SerializeField] private AudioFluxModuleManager moduleManager;

        [Header("Example Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private string[] gameStates = { "menu", "exploration", "combat" };
        [SerializeField] private float intensityChangeSpeed = 1f;

        private int currentStateIndex = 0;
        private float currentIntensity = 0.5f;

        private void Start()
        {
            // Subscribe to module events for demonstration
            ModuleEvents.OnModuleInitialized += OnModuleInitialized;
            ModuleEvents.OnModuleEnabled += OnModuleEnabled;
            ModuleEvents.OnModuleInitializationFailed += OnModuleInitializationFailed;
        }

        private void OnDestroy()
        {
            ModuleEvents.OnModuleInitialized -= OnModuleInitialized;
            ModuleEvents.OnModuleEnabled -= OnModuleEnabled;
            ModuleEvents.OnModuleInitializationFailed -= OnModuleInitializationFailed;
        }

        private void Update()
        {
            // Example input handling
            HandleInput();
        }

        private void HandleInput()
        {
            // Change game state
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeGameState();
            }

            // Change music intensity
            if (Input.GetKey(KeyCode.UpArrow))
            {
                ChangeIntensity(Time.deltaTime * intensityChangeSpeed);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                ChangeIntensity(-Time.deltaTime * intensityChangeSpeed);
            }

            // Play positioned sound with modules
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlayPositionedSoundWithModules();
            }

            // Play footstep sound
            if (Input.GetKeyDown(KeyCode.F))
            {
                PlayFootstepSound();
            }

            // Simulate collision
            if (Input.GetKeyDown(KeyCode.C))
            {
                SimulateCollision();
            }

            // Toggle module
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleModule();
            }
        }

        private void ChangeGameState()
        {
            currentStateIndex = (currentStateIndex + 1) % gameStates.Length;
            var newState = gameStates[currentStateIndex];

            Debug.Log($"Changing game state to: {newState}");

            // Use extension method to set game state
            musicService.SetGameState(newState);

            // Alternative: trigger the event directly
            // AdvancedAudioEvents.InvokeGameStateChanged(newState, null);
        }

        private void ChangeIntensity(float delta)
        {
            currentIntensity = Mathf.Clamp01(currentIntensity + delta);

            // Use extension method to set intensity
            musicService.SetMusicIntensity(currentIntensity);

            Debug.Log($"Music intensity: {currentIntensity:F2}");
        }

        private void PlayPositionedSoundWithModules()
        {
            if (playerTransform == null)
                return;

            var position = playerTransform.position + Random.insideUnitSphere * 5f;

            // Play sound with automatic module integration
            var playParams = new SoundPlayParamsBuilder()
                .AtPosition(position)
                .WithVolume(0.8f)
                .WithImportance(true) // Mark as important for LOD
                .Build();

            var handle = sfxService.PlaySoundWithModules("example_sound", playParams);

            if (handle != null)
            {
                Debug.Log($"Played positioned sound at {position}");

                // Request occlusion calculation
                handle.RequestOcclusion();

                // Check LOD level after a frame
                StartCoroutine(CheckLODAfterFrame(handle));
            }
        }

        private System.Collections.IEnumerator CheckLODAfterFrame(SoundHandle handle)
        {
            yield return null; // Wait one frame

            var lodLevel = handle.GetLODLevel();
            var isOccluded = handle.IsOccluded();

            Debug.Log($"Sound LOD: {lodLevel}, Occluded: {isOccluded}");
        }

        private void PlayFootstepSound()
        {
            if (playerTransform == null)
                return;

            // Get the surface material module
            var surfaceModule = moduleManager?.GetModule<Modules.Physics.SurfaceMaterialAudioModule>("physics.surfacematerial");

            if (surfaceModule != null)
            {
                var handle = surfaceModule.PlayFootstepSound(playerTransform.position);
                if (handle != null)
                {
                    Debug.Log("Played footstep sound with surface material detection");
                }
            }
            else
            {
                Debug.LogWarning("Surface Material Audio Module not found");
            }
        }

        private void SimulateCollision()
        {
            if (playerTransform == null)
                return;

            var position = playerTransform.position;
            var intensity = Random.Range(0.3f, 1f);

            // Simulate collision audio request
            sfxService.RequestCollisionAudio(null, intensity);

            Debug.Log($"Simulated collision with intensity: {intensity:F2}");
        }

        private void ToggleModule()
        {
            if (moduleManager == null)
                return;

            // Example: Toggle the occlusion module
            var occlusionModule = moduleManager.GetModule<Modules.Advanced3D.AudioOcclusionModule>("advanced3d.occlusion");

            if (occlusionModule != null)
            {
                if (occlusionModule.IsEnabled)
                {
                    moduleManager.DisableModule("advanced3d.occlusion");
                    Debug.Log("Disabled Audio Occlusion Module");
                }
                else
                {
                    moduleManager.EnableModule("advanced3d.occlusion");
                    Debug.Log("Enabled Audio Occlusion Module");
                }
            }
        }

        // Module event handlers
        private void OnModuleInitialized(IAudioFluxModule module)
        {
            Debug.Log($"Module initialized: {module.ModuleName} ({module.ModuleId})");
        }

        private void OnModuleEnabled(IAudioFluxModule module)
        {
            Debug.Log($"Module enabled: {module.ModuleName}");
        }

        private void OnModuleInitializationFailed(IAudioFluxModule module, System.Exception exception)
        {
            Debug.LogError($"Module initialization failed: {module.ModuleName} - {exception.Message}");
        }

        // Context menu methods for testing
        [ContextMenu("List All Modules")]
        public void ListAllModules()
        {
            if (moduleManager == null)
            {
                Debug.LogWarning("Module Manager not found");
                return;
            }

            Debug.Log("=== AudioFlux Modules ===");
            foreach (var kvp in moduleManager.Modules)
            {
                var module = kvp.Value;
                var metadata = moduleManager.ModuleMetadata.ContainsKey(kvp.Key) ? moduleManager.ModuleMetadata[kvp.Key] : null;

                Debug.Log($"• {module.ModuleName} ({module.ModuleId})");
                Debug.Log($"  Version: {module.ModuleVersion}");
                Debug.Log($"  Enabled: {module.IsEnabled}");

                if (metadata != null)
                {
                    Debug.Log($"  Category: {metadata.Category}");
                    Debug.Log($"  Description: {metadata.Description}");
                }

                Debug.Log("");
            }
        }

        [ContextMenu("Test State Transitions")]
        public void TestStateTransitions()
        {
            if (musicService == null)
                return;

            StartCoroutine(StateTransitionSequence());
        }

        private System.Collections.IEnumerator StateTransitionSequence()
        {
            foreach (var state in gameStates)
            {
                Debug.Log($"Transitioning to: {state}");
                musicService.TransitionToMusicState(state);
                yield return new WaitForSeconds(3f);
            }
        }

        [ContextMenu("Test Intensity Scaling")]
        public void TestIntensityScaling()
        {
            if (musicService == null)
                return;

            StartCoroutine(IntensityScalingSequence());
        }

        private System.Collections.IEnumerator IntensityScalingSequence()
        {
            for (float intensity = 0f; intensity <= 1f; intensity += 0.2f)
            {
                Debug.Log($"Setting intensity to: {intensity:F1}");
                musicService.SetMusicIntensity(intensity);
                yield return new WaitForSeconds(2f);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("AudioFlux Module System Example", GUI.skin.box);

            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("1 - Change Game State");
            GUILayout.Label("↑/↓ - Change Music Intensity");
            GUILayout.Label("Space - Play Positioned Sound");
            GUILayout.Label("F - Play Footstep Sound");
            GUILayout.Label("C - Simulate Collision");
            GUILayout.Label("M - Toggle Occlusion Module");

            GUILayout.Space(10);
            GUILayout.Label($"Current State: {gameStates[currentStateIndex]}");
            GUILayout.Label($"Current Intensity: {currentIntensity:F2}");

            if (musicService != null)
            {
                GUILayout.Label($"Music State: {musicService.GetCurrentMusicState()}");
                GUILayout.Label($"Music Intensity: {musicService.GetCurrentMusicIntensity():F2}");
            }

            GUILayout.EndArea();
        }
    }
}
