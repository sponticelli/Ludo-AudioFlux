using UnityEngine;
using UnityEditor;
using Ludo.AudioFlux.Modules;
using System.Linq;

namespace Ludo.AudioFlux.Editor
{
    /// <summary>
    /// Custom editor for the AudioFlux Module Manager
    /// </summary>
    [CustomEditor(typeof(AudioFluxModuleManager))]
    public class ModuleManagerEditor : UnityEditor.Editor
    {
        private bool _showModuleList = true;
        private bool _showModuleSettings = false;
        private string _selectedModuleId = null;

        public override void OnInspectorGUI()
        {
            var moduleManager = (AudioFluxModuleManager)target;

            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Module System", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Module information is only available during play mode.", MessageType.Info);
                return;
            }

            // Module discovery button
            if (GUILayout.Button("Discover Modules"))
            {
                moduleManager.DiscoverModules();
            }

            // Module list
            _showModuleList = EditorGUILayout.Foldout(_showModuleList, $"Modules ({moduleManager.Modules.Count})");
            if (_showModuleList)
            {
                EditorGUI.indentLevel++;
                DrawModuleList(moduleManager);
                EditorGUI.indentLevel--;
            }

            // Module settings
            if (!string.IsNullOrEmpty(_selectedModuleId))
            {
                EditorGUILayout.Space();
                _showModuleSettings = EditorGUILayout.Foldout(_showModuleSettings, "Module Settings");
                if (_showModuleSettings)
                {
                    EditorGUI.indentLevel++;
                    DrawModuleSettings(moduleManager, _selectedModuleId);
                    EditorGUI.indentLevel--;
                }
            }

            // Repaint to keep the inspector updated
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void DrawModuleList(AudioFluxModuleManager moduleManager)
        {
            if (moduleManager.Modules.Count == 0)
            {
                EditorGUILayout.LabelField("No modules found", EditorStyles.miniLabel);
                return;
            }

            foreach (var kvp in moduleManager.Modules)
            {
                var moduleId = kvp.Key;
                var module = kvp.Value;
                var metadata = moduleManager.ModuleMetadata.ContainsKey(moduleId) ? moduleManager.ModuleMetadata[moduleId] : null;

                EditorGUILayout.BeginHorizontal();

                // Module selection toggle
                var isSelected = _selectedModuleId == moduleId;
                var newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                if (newSelected != isSelected)
                {
                    _selectedModuleId = newSelected ? moduleId : null;
                }

                // Module info
                var displayName = metadata?.ModuleName ?? module.ModuleName;
                var category = metadata?.Category ?? "Unknown";
                var statusColor = module.IsEnabled ? Color.green : Color.gray;

                var originalColor = GUI.color;
                GUI.color = statusColor;
                EditorGUILayout.LabelField($"[{category}] {displayName}", EditorStyles.miniLabel);
                GUI.color = originalColor;

                // Enable/Disable button
                var buttonText = module.IsEnabled ? "Disable" : "Enable";
                if (GUILayout.Button(buttonText, EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    if (module.IsEnabled)
                    {
                        moduleManager.DisableModule(moduleId);
                    }
                    else
                    {
                        moduleManager.EnableModule(moduleId);
                    }
                }

                EditorGUILayout.EndHorizontal();

                // Module details (when selected)
                if (isSelected)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("Module ID:", moduleId, EditorStyles.miniLabel);
                    EditorGUILayout.LabelField("Version:", module.ModuleVersion.ToString(), EditorStyles.miniLabel);

                    if (metadata != null)
                    {
                        if (!string.IsNullOrEmpty(metadata.Description))
                        {
                            EditorGUILayout.LabelField("Description:", metadata.Description, EditorStyles.wordWrappedMiniLabel);
                        }

                        if (!string.IsNullOrEmpty(metadata.Author))
                        {
                            EditorGUILayout.LabelField("Author:", metadata.Author, EditorStyles.miniLabel);
                        }

                        if (metadata.Dependencies.Length > 0)
                        {
                            EditorGUILayout.LabelField("Dependencies:", string.Join(", ", metadata.Dependencies), EditorStyles.miniLabel);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawModuleSettings(AudioFluxModuleManager moduleManager, string moduleId)
        {
            var module = moduleManager.GetModule<IAudioFluxModule>(moduleId);
            if (module == null)
                return;

            EditorGUILayout.LabelField($"Settings for {module.ModuleName}", EditorStyles.boldLabel);

            // Use reflection to find and display module settings
            var moduleType = module.GetType();
            var properties = moduleType.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(ModuleSettingAttribute), false).Length > 0)
                .ToArray();

            if (properties.Length == 0)
            {
                EditorGUILayout.LabelField("No configurable settings", EditorStyles.miniLabel);
                return;
            }

            foreach (var property in properties)
            {
                var settingAttr = (ModuleSettingAttribute)property.GetCustomAttributes(typeof(ModuleSettingAttribute), false)[0];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(settingAttr.DisplayName, GUILayout.Width(150));

                // Draw appropriate control based on property type
                var currentValue = property.GetValue(module);
                object newValue = null;

                if (property.PropertyType == typeof(bool))
                {
                    newValue = EditorGUILayout.Toggle((bool)currentValue);
                }
                else if (property.PropertyType == typeof(float))
                {
                    if (settingAttr.MinValue != null && settingAttr.MaxValue != null)
                    {
                        newValue = EditorGUILayout.Slider((float)currentValue, (float)settingAttr.MinValue, (float)settingAttr.MaxValue);
                    }
                    else
                    {
                        newValue = EditorGUILayout.FloatField((float)currentValue);
                    }
                }
                else if (property.PropertyType == typeof(int))
                {
                    if (settingAttr.MinValue != null && settingAttr.MaxValue != null)
                    {
                        newValue = EditorGUILayout.IntSlider((int)currentValue, (int)settingAttr.MinValue, (int)settingAttr.MaxValue);
                    }
                    else
                    {
                        newValue = EditorGUILayout.IntField((int)currentValue);
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    newValue = EditorGUILayout.TextField((string)currentValue);
                }
                else if (property.PropertyType == typeof(LayerMask))
                {
                    newValue = EditorGUILayout.MaskField((LayerMask)currentValue, UnityEditorInternal.InternalEditorUtility.layers);
                }

                EditorGUILayout.EndHorizontal();

                // Apply changes
                if (newValue != null && !newValue.Equals(currentValue))
                {
                    property.SetValue(module, newValue);
                    EditorUtility.SetDirty(target);
                }

                // Show description if available
                if (!string.IsNullOrEmpty(settingAttr.Description))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(settingAttr.Description, EditorStyles.helpBox);
                    EditorGUI.indentLevel--;
                }
            }
        }
    }

    /// <summary>
    /// Menu items for creating AudioFlux components with module support
    /// </summary>
    public static class ModuleMenuItems
    {
        [MenuItem("GameObject/Ludo/AudioFlux/Module Manager", false, 10)]
        public static void CreateModuleManager()
        {
            var go = new GameObject("AudioFlux Module Manager");
            go.AddComponent<AudioFluxModuleManager>();

            // Try to find and assign services automatically
            var musicService = UnityEngine.Object.FindObjectOfType<MusicService>();
            var sfxService = UnityEngine.Object.FindObjectOfType<SfxService>();

            var moduleManager = go.GetComponent<AudioFluxModuleManager>();

            if (musicService != null)
            {
                var serializedObject = new SerializedObject(moduleManager);
                var musicServiceProp = serializedObject.FindProperty("musicService");
                musicServiceProp.objectReferenceValue = musicService;
                serializedObject.ApplyModifiedProperties();
            }

            if (sfxService != null)
            {
                var serializedObject = new SerializedObject(moduleManager);
                var sfxServiceProp = serializedObject.FindProperty("sfxService");
                sfxServiceProp.objectReferenceValue = sfxService;
                serializedObject.ApplyModifiedProperties();
            }

            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create AudioFlux Module Manager");
        }

        [MenuItem("GameObject/Ludo/AudioFlux/Surface Material Component", false, 11)]
        public static void CreateSurfaceMaterialComponent()
        {
            var selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects to add Surface Material components to.", "OK");
                return;
            }

            foreach (var go in selectedObjects)
            {
                if (go.GetComponent<Modules.Physics.SurfaceMaterialComponent>() == null)
                {
                    Undo.AddComponent<Modules.Physics.SurfaceMaterialComponent>(go);
                }
            }
        }
    }
}
