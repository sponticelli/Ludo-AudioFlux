using UnityEngine;
using UnityEditor;


namespace Ludo.AudioFlux
{
    // Custom inspector for SoundDefinition
    [CustomEditor(typeof(SoundDefinition))]
    public class SoundDefinitionEditor : UnityEditor.Editor
    {
        private AudioSource previewSource;
        private bool isPlaying = false;

        private void OnEnable()
        {
            // Create a temporary audio source for preview
            GameObject previewObj =
                EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave);
            previewSource = previewObj.AddComponent<AudioSource>();
            previewSource.playOnAwake = false;
        }

        private void OnDisable()
        {
            if (previewSource == null) return;
            previewSource.Stop();
            DestroyImmediate(previewSource.gameObject);
        }

        public override void OnInspectorGUI()
        {
            SoundDefinition soundDef = (SoundDefinition)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            // Preview section
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            var audioClip = soundDef.AudioClip;

            if (audioClip != null)
            {
                EditorGUILayout.BeginHorizontal();

                // Play/Stop button
                string buttonText = isPlaying ? "Stop" : "Play";
                if (GUILayout.Button(buttonText, GUILayout.Width(60)))
                {
                    if (isPlaying)
                    {
                        StopPreview();
                    }
                    else
                    {
                        PlayPreview(soundDef, audioClip);
                    }
                }

                // Volume slider
                EditorGUI.BeginChangeCheck();
                float previewVolume = EditorGUILayout.Slider("Preview Volume", previewSource.volume, 0f, 1f);
                if (EditorGUI.EndChangeCheck() && previewSource.isPlaying)
                {
                    previewSource.volume = previewVolume;
                }

                EditorGUILayout.EndHorizontal();

                // Audio info
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Duration: {audioClip.length:F2}s");
                EditorGUILayout.LabelField($"Frequency: {audioClip.frequency} Hz");
                EditorGUILayout.LabelField($"Channels: {audioClip.channels}");
                EditorGUILayout.LabelField($"Samples: {audioClip.samples:N0}");

                if (isPlaying && previewSource.isPlaying)
                {
                    float progress = previewSource.time / audioClip.length;
                    EditorGUILayout.Space(5);
                    Rect progressRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(progressRect, progress, $"{previewSource.time:F1}s / {audioClip.length:F1}s");
                }

                EditorGUILayout.EndVertical();

                // 3D Audio visualization
                if (soundDef.SpatialBlend > 0f)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("3D Audio Settings", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Spatial Blend: {soundDef.SpatialBlend:P0}");
                    EditorGUILayout.LabelField($"Max Distance: {soundDef.MaxDistance}m");
                    EditorGUILayout.LabelField($"Rolloff: {soundDef.RolloffMode}");
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign an AudioClip to enable preview", MessageType.Info);
            }

            // Update preview playback state
            if (isPlaying && !previewSource.isPlaying)
            {
                isPlaying = false;
                Repaint();
            }

            // Auto-repaint while playing for progress bar
            if (isPlaying)
            {
                Repaint();
            }
        }

        private void PlayPreview(SoundDefinition soundDef, AudioClip audioClip)
        {
            if (previewSource != null && audioClip != null)
            {
                previewSource.clip = audioClip;
                previewSource.volume = soundDef.Volume;
                previewSource.pitch = soundDef.Pitch;
                previewSource.spatialBlend = soundDef.SpatialBlend;
                previewSource.loop = soundDef.Loop;
                previewSource.rolloffMode = soundDef.RolloffMode;
                previewSource.maxDistance = soundDef.MaxDistance;
                previewSource.priority = soundDef.priority;
                previewSource.outputAudioMixerGroup = soundDef.MixerGroup;

                previewSource.Play();
                isPlaying = true;
            }
        }

        private void StopPreview()
        {
            if (previewSource != null)
            {
                previewSource.Stop();
                isPlaying = false;
            }
        }
    }
}