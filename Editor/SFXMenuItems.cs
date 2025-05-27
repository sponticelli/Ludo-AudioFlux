using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    public static class SFXMenuItems
    {
        [MenuItem("GameObject/Ludo/AudioFlux/SFX Service", false, 10)]
        static void CreateSFXService(MenuCommand menuCommand)
        {
            GameObject sfxServiceObj = new GameObject("SFX Service");
            sfxServiceObj.AddComponent<SfxService>();
            
            // Place it in the hierarchy
            GameObjectUtility.SetParentAndAlign(sfxServiceObj, menuCommand.context as GameObject);
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(sfxServiceObj, "Create SFX Service");
            Selection.activeObject = sfxServiceObj;
        }
        
    }
}