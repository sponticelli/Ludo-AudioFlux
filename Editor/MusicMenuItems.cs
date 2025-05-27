using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    public static class MusicMenuItems
    {
        [MenuItem("GameObject/Ludo/AudioFlux/Music Service", false, 11)]
        static void CreateMusicService(MenuCommand menuCommand)
        {
            GameObject musicServiceObj = new GameObject("Music Service");
            musicServiceObj.AddComponent<MusicService>();

            // Place it in the hierarchy
            GameObjectUtility.SetParentAndAlign(musicServiceObj, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(musicServiceObj, "Create Music Service");
            Selection.activeObject = musicServiceObj;
        }

        [MenuItem("GameObject/Ludo/AudioFlux/Play Music Component", false, 12)]
        static void CreatePlayMusicComponent(MenuCommand menuCommand)
        {
            GameObject playMusicObj = new GameObject("Play Music");
            playMusicObj.AddComponent<PlayMusicComponent>();

            // Place it in the hierarchy
            GameObjectUtility.SetParentAndAlign(playMusicObj, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(playMusicObj, "Create Play Music Component");
            Selection.activeObject = playMusicObj;
        }

        [MenuItem("Assets/Create/Ludo/AudioFlux/Music Definition", false, 10)]
        static void CreateMusicDefinition()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New MusicDefinition.asset");

            var musicDefinition = ScriptableObject.CreateInstance<MusicDefinition>();
            AssetDatabase.CreateAsset(musicDefinition, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = musicDefinition;
        }
    }
}
