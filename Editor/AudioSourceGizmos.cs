using UnityEditor;
using UnityEngine;

namespace Ludo.AudioFlux
{
    public class AudioSourceGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawAudioSourceGizmo(AudioSource audioSource, GizmoType gizmoType)
        {
            if (audioSource.spatialBlend > 0f)
            {
                // Draw max distance sphere
                Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                Gizmos.DrawSphere(audioSource.transform.position, audioSource.maxDistance);
                
                // Draw wireframe
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(audioSource.transform.position, audioSource.maxDistance);
                
                // Draw inner sphere for minimum distance if using linear rolloff
                if (audioSource.rolloffMode == AudioRolloffMode.Linear)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                    Gizmos.DrawSphere(audioSource.transform.position, audioSource.minDistance);
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(audioSource.transform.position, audioSource.minDistance);
                }
            }
        }
    }
}