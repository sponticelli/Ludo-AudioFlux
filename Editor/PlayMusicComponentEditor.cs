using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    [CustomEditor(typeof(PlayMusicComponent))]
    public class PlayMusicComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty musicIdProp;
        private SerializedProperty playOnStartProp;
        private SerializedProperty playOnEnableProp;
        private SerializedProperty volumeMultiplierProp;
        private SerializedProperty pitchMultiplierProp;
        private SerializedProperty playIntroProp;
        private SerializedProperty enableLayersProp;
        private SerializedProperty startTimeProp;
        private SerializedProperty useCrossfadeProp;
        private SerializedProperty crossfadeDurationProp;
        private SerializedProperty musicServiceProp;
        
        private string[] availableMusicIds = new string[0];
        private int selectedMusicIndex = 0;
        private bool useDropdown = false;
        
        private void OnEnable()
        {
            musicIdProp = serializedObject.FindProperty("musicId");
            playOnStartProp = serializedObject.FindProperty("playOnStart");
            playOnEnableProp = serializedObject.FindProperty("playOnEnable");
            volumeMultiplierProp = serializedObject.FindProperty("volumeMultiplier");
            pitchMultiplierProp = serializedObject.FindProperty("pitchMultiplier");
            playIntroProp = serializedObject.FindProperty("playIntro");
            enableLayersProp = serializedObject.FindProperty("enableLayers");
            startTimeProp = serializedObject.FindProperty("startTime");
            useCrossfadeProp = serializedObject.FindProperty("useCrossfade");
            crossfadeDurationProp = serializedObject.FindProperty("crossfadeDuration");
            musicServiceProp = serializedObject.FindProperty("musicService");
            
            RefreshAvailableMusicIds();
        }
        
        public override void OnInspectorGUI()
        {
            PlayMusicComponent component = (PlayMusicComponent)target;
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Music Settings", EditorStyles.boldLabel);
            
            // Music ID selection with dropdown support
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Music ID", GUILayout.Width(60));
            
            if (availableMusicIds.Length > 0 && useDropdown)
            {
                // Find current music ID in available list
                string currentId = musicIdProp.stringValue;
                selectedMusicIndex = System.Array.IndexOf(availableMusicIds, currentId);
                if (selectedMusicIndex < 0) selectedMusicIndex = 0;
                
                selectedMusicIndex = EditorGUILayout.Popup(selectedMusicIndex, availableMusicIds);
                musicIdProp.stringValue = availableMusicIds[selectedMusicIndex];
                
                if (GUILayout.Button("Manual", GUILayout.Width(60)))
                {
                    useDropdown = false;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(musicIdProp, GUIContent.none);
                
                if (availableMusicIds.Length > 0)
                {
                    if (GUILayout.Button("Dropdown", GUILayout.Width(80)))
                    {
                        useDropdown = true;
                        RefreshAvailableMusicIds();
                    }
                }
            }
            
            if (GUILayout.Button("↻", GUILayout.Width(25)))
            {
                RefreshAvailableMusicIds();
            }
            EditorGUILayout.EndHorizontal();
            
            // Show help if no music service found
            if (availableMusicIds.Length == 0)
            {
                EditorGUILayout.HelpBox("No MusicService found in scene or no music configured. Add a MusicService to the scene and configure its music library.", MessageType.Info);
            }
            
            EditorGUILayout.PropertyField(playOnStartProp);
            EditorGUILayout.PropertyField(playOnEnableProp);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Playback Parameters", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(volumeMultiplierProp);
            EditorGUILayout.PropertyField(pitchMultiplierProp);
            EditorGUILayout.PropertyField(playIntroProp);
            EditorGUILayout.PropertyField(enableLayersProp);
            EditorGUILayout.PropertyField(startTimeProp);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Crossfade Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(useCrossfadeProp);
            if (useCrossfadeProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(crossfadeDurationProp);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Service Reference", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(musicServiceProp);
            
            if (musicServiceProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Music Service will be automatically found if not assigned.", MessageType.Info);
            }
            
            // Runtime testing
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                {
                    component.PlayMusic();
                }
                if (GUILayout.Button("Stop"))
                {
                    component.StopMusic();
                }
                if (GUILayout.Button("Crossfade"))
                {
                    component.CrossfadeToMusic();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Pause"))
                {
                    component.PauseMusic();
                }
                if (GUILayout.Button("Resume"))
                {
                    component.ResumeMusic();
                }
                EditorGUILayout.EndHorizontal();
                
                // Status info
                EditorGUILayout.Space();
                bool isPlaying = component.IsPlaying();
                EditorGUILayout.LabelField($"Is Playing: {isPlaying}");
                
                var handle = component.GetCurrentHandle();
                if (handle != null)
                {
                    EditorGUILayout.LabelField($"Current Music: {handle.musicId}");
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void RefreshAvailableMusicIds()
        {
            PlayMusicComponent component = (PlayMusicComponent)target;
            
            // Try to find MusicService
            MusicService musicService = component.GetComponent<MusicService>();
            if (musicService == null)
            {
                musicService = FindObjectOfType<MusicService>();
            }
            
            if (musicService != null)
            {
                if (Application.isPlaying)
                {
                    availableMusicIds = musicService.GetAvailableMusicIds();
                }
                else
                {
                    // In edit mode, try to get music IDs from the serialized music library
                    SerializedObject so = new SerializedObject(musicService);
                    SerializedProperty musicLibraryProp = so.FindProperty("musicLibrary");
                    
                    if (musicLibraryProp != null && musicLibraryProp.isArray)
                    {
                        var musicIds = new System.Collections.Generic.List<string>();
                        for (int i = 0; i < musicLibraryProp.arraySize; i++)
                        {
                            var element = musicLibraryProp.GetArrayElementAtIndex(i);
                            if (element.objectReferenceValue != null)
                            {
                                musicIds.Add(element.objectReferenceValue.name);
                            }
                        }
                        availableMusicIds = musicIds.ToArray();
                    }
                }
            }
            else
            {
                availableMusicIds = new string[0];
            }
        }
    }
}
