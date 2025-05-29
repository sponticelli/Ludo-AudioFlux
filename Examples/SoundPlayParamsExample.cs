using UnityEngine;
using Ludo.AudioFlux;

namespace Ludo.AudioFlux.Examples
{
    /// <summary>
    /// Example demonstrating the improved SoundPlayParams usage
    /// </summary>
    public class SoundPlayParamsExample : MonoBehaviour
    {
        [SerializeField] private SfxService sfxService;
        [SerializeField] private string soundId = "SoundDefinition";
        [SerializeField] private Transform targetToFollow;

        private void Start()
        {
            if (sfxService == null)
            {
                sfxService = FindFirstObjectByType<SfxService>();
            }
        }

        [ContextMenu("Play Sound at Position")]
        public void PlaySoundAtPosition()
        {
            // Old way (still works but deprecated)
            // var oldParams = new SoundPlayParams { position = transform.position };
            
            // New way - using static factory method
            var playParams = SoundPlayParams.At(transform.position);
            sfxService.PlaySound(soundId, playParams);
        }

        [ContextMenu("Play Sound Following Target")]
        public void PlaySoundFollowingTarget()
        {
            if (targetToFollow == null)
            {
                Debug.LogWarning("No target to follow set!");
                return;
            }

            // New way - using static factory method
            var playParams = SoundPlayParams.Following(targetToFollow);
            sfxService.PlaySound(soundId, playParams);
        }

        [ContextMenu("Play Sound with Volume")]
        public void PlaySoundWithVolume()
        {
            // New way - using static factory method
            var playParams = SoundPlayParams.WithVolume(0.5f);
            sfxService.PlaySound(soundId, playParams);
        }

        [ContextMenu("Play Sound with Complex Parameters")]
        public void PlaySoundWithComplexParameters()
        {
            // New way - using builder pattern for complex combinations
            var playParams = new SoundPlayParamsBuilder()
                .AtPosition(transform.position + Vector3.up * 2f)
                .WithVolume(0.8f)
                .WithPitch(1.2f)
                .WithLoop(false)
                .OnComplete(handle => Debug.Log($"Sound {handle.soundId} completed!"))
                .Build();

            sfxService.PlaySound(soundId, playParams);
        }

        [ContextMenu("Demonstrate Type Safety")]
        public void DemonstrateTypeSafety()
        {
            // This is now impossible - you can't accidentally set both position and followTarget
            // The enum makes it clear what mode you're in
            
            var positionParams = SoundPlayParams.At(Vector3.zero);
            Debug.Log($"Position mode: {positionParams.PositionMode}"); // AtPosition
            Debug.Log($"Position: {positionParams.Position}"); // (0, 0, 0)
            
            var followParams = SoundPlayParams.Following(transform);
            Debug.Log($"Follow mode: {followParams.PositionMode}"); // FollowTarget
            Debug.Log($"Follow target: {followParams.FollowTarget.name}"); // GameObject name
            
            // You can't accidentally mix them anymore!
        }
    }
}