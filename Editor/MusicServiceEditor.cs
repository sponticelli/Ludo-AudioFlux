using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    [CustomEditor(typeof(MusicService))]
    public class MusicServiceEditor : UnityEditor.Editor
    {
        private string testMusicId = "";
        private int selectedMusicIndex = 0;
        private string[] availableMusicIds = new string[0];
        private float testVolume = 1f;
        private float testPitch = 1f;
        private float testCrossfadeDuration = 1f;
        private float testDuckingLevel = 0.5f;
        private float testDuckingTime = 0.5f;
        private bool useManualInput = false;
        private bool showAdvancedControls = false;
        private bool enableIntro = true;
        private bool enableLayers = true;
        private float startTime = 0f;

        public override void OnInspectorGUI()
        {
            MusicService musicService = (MusicService)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // Runtime testing section
            EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");

                // Get available music IDs
                availableMusicIds = musicService.GetAvailableMusicIds();

                // Music ID selection
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Music ID", GUILayout.Width(60));

                if (availableMusicIds.Length > 0 && !useManualInput)
                {
                    // Ensure selected index is valid
                    if (selectedMusicIndex >= availableMusicIds.Length)
                        selectedMusicIndex = 0;

                    selectedMusicIndex = EditorGUILayout.Popup(selectedMusicIndex, availableMusicIds);
                    testMusicId = availableMusicIds[selectedMusicIndex];

                    if (GUILayout.Button("Manual", GUILayout.Width(60)))
                    {
                        useManualInput = true;
                    }
                }
                else
                {
                    // Manual input or no music available
                    testMusicId = EditorGUILayout.TextField(testMusicId);

                    if (availableMusicIds.Length > 0)
                    {
                        if (GUILayout.Button("Dropdown", GUILayout.Width(80)))
                        {
                            useManualInput = false;
                            // Try to find current testMusicId in available music
                            for (int i = 0; i < availableMusicIds.Length; i++)
                            {
                                if (availableMusicIds[i] == testMusicId)
                                {
                                    selectedMusicIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Show help text if no music available
                if (availableMusicIds.Length == 0)
                {
                    EditorGUILayout.HelpBox("No music configured in the Music Library. Add MusicDefinition assets to the musicLibrary array.", MessageType.Warning);
                }

                // Basic controls
                testVolume = EditorGUILayout.Slider("Volume", testVolume, 0f, 1f);
                testPitch = EditorGUILayout.Slider("Pitch", testPitch, 0.1f, 3f);

                // Advanced controls foldout
                showAdvancedControls = EditorGUILayout.Foldout(showAdvancedControls, "Advanced Controls");
                if (showAdvancedControls)
                {
                    EditorGUI.indentLevel++;
                    enableIntro = EditorGUILayout.Toggle("Play Intro", enableIntro);
                    enableLayers = EditorGUILayout.Toggle("Enable Layers", enableLayers);
                    startTime = EditorGUILayout.FloatField("Start Time", startTime);
                    testCrossfadeDuration = EditorGUILayout.Slider("Crossfade Duration", testCrossfadeDuration, 0.1f, 5f);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(5);

                // Main playback controls
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Play Music"))
                {
                    if (!string.IsNullOrEmpty(testMusicId))
                    {
                        var playParams = new MusicPlayParamsBuilder()
                            .WithVolume(testVolume)
                            .WithPitch(testPitch)
                            .WithLayers(enableLayers);

                        if (!enableIntro)
                            playParams.SkipIntro();

                        if (startTime > 0f)
                            playParams.FromTime(startTime);

                        musicService.PlayMusic(testMusicId, playParams.Build());
                    }
                }

                if (GUILayout.Button("Crossfade To"))
                {
                    if (!string.IsNullOrEmpty(testMusicId))
                    {
                        var playParams = new MusicPlayParamsBuilder()
                            .WithVolume(testVolume)
                            .WithPitch(testPitch)
                            .WithLayers(enableLayers);

                        if (!enableIntro)
                            playParams.SkipIntro();

                        musicService.CrossfadeTo(testMusicId, testCrossfadeDuration, playParams.Build());
                    }
                }

                if (GUILayout.Button("Stop"))
                {
                    musicService.StopMusic();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Global controls
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Pause"))
                {
                    musicService.PauseMusic();
                }
                if (GUILayout.Button("Resume"))
                {
                    musicService.ResumeMusic();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // Ducking controls
                EditorGUILayout.LabelField("Ducking Controls", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                testDuckingLevel = EditorGUILayout.Slider("Ducking Level", testDuckingLevel, 0f, 1f);
                testDuckingTime = EditorGUILayout.FloatField("Time", testDuckingTime, GUILayout.Width(60));
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    musicService.SetDucking(testDuckingLevel, testDuckingTime);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                // Debug info
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Debug Info", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");

                bool isPlaying = musicService.IsPlaying();
                EditorGUILayout.LabelField($"Music Playing: {isPlaying}");

                if (!string.IsNullOrEmpty(testMusicId))
                {
                    bool isSpecificPlaying = musicService.IsPlaying(testMusicId);
                    EditorGUILayout.LabelField($"'{testMusicId}' is playing: {isSpecificPlaying}");
                }

                var currentMusic = musicService.GetCurrentMusic();
                if (currentMusic != null)
                {
                    EditorGUILayout.LabelField($"Current Music: {currentMusic.musicId}");
                    EditorGUILayout.LabelField($"Has Intro: {currentMusic.definition?.HasIntro ?? false}");
                    EditorGUILayout.LabelField($"Has Layers: {currentMusic.definition?.HasLayers ?? false}");
                    if (currentMusic.definition != null)
                    {
                        EditorGUILayout.LabelField($"BPM: {currentMusic.definition.BPM}");
                        EditorGUILayout.LabelField($"Duration: {currentMusic.definition.TotalDuration:F2}s");
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Current Music: None");
                }

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test music at runtime", MessageType.Info);
            }
        }
    }
}
