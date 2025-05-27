using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    [CustomEditor(typeof(SfxService))]
    public class SFXServiceEditor : UnityEditor.Editor
    {
        private string testSoundId = "";
        private int selectedSoundIndex = 0;
        private string[] availableSoundIds = new string[0];
        private Vector3 testPosition = Vector3.zero;
        private float testVolume = 1f;
        private float testPitch = 1f;
        private bool useManualInput = false;

        public override void OnInspectorGUI()
        {
            SfxService sfxService = (SfxService)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // Runtime testing section
            EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");

                // Get available sound IDs
                availableSoundIds = sfxService.GetAvailableSoundIds();

                // Sound ID selection
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sound ID", GUILayout.Width(60));

                if (availableSoundIds.Length > 0 && !useManualInput)
                {
                    // Ensure selected index is valid
                    if (selectedSoundIndex >= availableSoundIds.Length)
                        selectedSoundIndex = 0;

                    selectedSoundIndex = EditorGUILayout.Popup(selectedSoundIndex, availableSoundIds);
                    testSoundId = availableSoundIds[selectedSoundIndex];

                    if (GUILayout.Button("Manual", GUILayout.Width(60)))
                    {
                        useManualInput = true;
                    }
                }
                else
                {
                    // Manual input or no sounds available
                    testSoundId = EditorGUILayout.TextField(testSoundId);

                    if (availableSoundIds.Length > 0)
                    {
                        if (GUILayout.Button("Dropdown", GUILayout.Width(80)))
                        {
                            useManualInput = false;
                            // Try to find current testSoundId in available sounds
                            for (int i = 0; i < availableSoundIds.Length; i++)
                            {
                                if (availableSoundIds[i] == testSoundId)
                                {
                                    selectedSoundIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Show help text if no sounds available
                if (availableSoundIds.Length == 0)
                {
                    EditorGUILayout.HelpBox("No sounds configured in the Sound Library. Add SoundDefinition assets to the soundLibrary array.", MessageType.Warning);
                }

                testPosition = EditorGUILayout.Vector3Field("Test Position", testPosition);
                testVolume = EditorGUILayout.Slider("Volume", testVolume, 0f, 1f);
                testPitch = EditorGUILayout.Slider("Pitch", testPitch, 0.1f, 3f);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Play Sound"))
                {
                    if (!string.IsNullOrEmpty(testSoundId))
                    {
                        var playParams = new SoundPlayParamsBuilder()
                            .AtPosition(testPosition)
                            .WithVolume(testVolume)
                            .WithPitch(testPitch)
                            .Build();
                        sfxService.PlaySound(testSoundId, playParams);
                    }
                }

                if (GUILayout.Button("Stop All"))
                {
                    sfxService.StopAllSounds();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Global controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Pause All"))
                {
                    sfxService.PauseAll();
                }
                if (GUILayout.Button("Resume All"))
                {
                    sfxService.ResumeAll();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                // Debug info
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                if (!string.IsNullOrEmpty(testSoundId))
                {
                    bool isPlaying = sfxService.IsPlaying(testSoundId);
                    EditorGUILayout.LabelField($"'{testSoundId}' is playing: {isPlaying}");
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test sounds at runtime", MessageType.Info);
            }
        }
    }
}