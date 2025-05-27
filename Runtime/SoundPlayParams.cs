using System;
using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Defines how the audio source should be positioned
    /// </summary>
    public enum SoundPositionMode
    {
        /// <summary>
        /// Use default positioning (no special positioning)
        /// </summary>
        Default,
        /// <summary>
        /// Position at a specific world position
        /// </summary>
        AtPosition,
        /// <summary>
        /// Follow a transform (parented to it)
        /// </summary>
        FollowTarget
    }

    /// <summary>
    /// Sound playback parameters with improved type safety
    /// </summary>
    public struct SoundPlayParams
    {
        // Positioning
        internal SoundPositionMode positionMode;
        internal Vector3 position;
        internal Transform followTarget;

        // Audio parameters
        public float? volumeMultiplier;
        public float? pitchMultiplier;
        public bool? loop;
        public Action<SoundHandle> onComplete;

        /// <summary>
        /// Gets the positioning mode
        /// </summary>
        public SoundPositionMode PositionMode => positionMode;

        /// <summary>
        /// Gets the world position (only valid when PositionMode is AtPosition)
        /// </summary>
        public Vector3 Position => position;

        /// <summary>
        /// Gets the follow target (only valid when PositionMode is FollowTarget)
        /// </summary>
        public Transform FollowTarget => followTarget;
        

        /// <summary>
        /// Creates default sound parameters
        /// </summary>
        public static SoundPlayParams Default => new SoundPlayParams { positionMode = SoundPositionMode.Default };

        /// <summary>
        /// Creates parameters for playing at a specific position
        /// </summary>
        public static SoundPlayParams At(Vector3 position) => new SoundPlayParams
        {
            positionMode = SoundPositionMode.AtPosition,
            position = position
        };

        /// <summary>
        /// Creates parameters for following a transform
        /// </summary>
        public static SoundPlayParams Following(Transform target) => new SoundPlayParams
        {
            positionMode = SoundPositionMode.FollowTarget,
            followTarget = target
        };

        /// <summary>
        /// Creates parameters with volume multiplier
        /// </summary>
        public static SoundPlayParams WithVolume(float volume) => new SoundPlayParams
        {
            positionMode = SoundPositionMode.Default,
            volumeMultiplier = volume
        };

        /// <summary>
        /// Creates parameters with pitch multiplier
        /// </summary>
        public static SoundPlayParams WithPitch(float pitch) => new SoundPlayParams
        {
            positionMode = SoundPositionMode.Default,
            pitchMultiplier = pitch
        };

        /// <summary>
        /// Creates parameters with loop setting
        /// </summary>
        public static SoundPlayParams WithLoop(bool shouldLoop) => new SoundPlayParams
        {
            positionMode = SoundPositionMode.Default,
            loop = shouldLoop
        };
    }

    /// <summary>
    /// Builder class for creating complex SoundPlayParams
    /// </summary>
    public class SoundPlayParamsBuilder
    {
        private SoundPlayParams _params;

        public SoundPlayParamsBuilder()
        {
            _params = SoundPlayParams.Default;
        }

        /// <summary>
        /// Set position for the sound
        /// </summary>
        public SoundPlayParamsBuilder AtPosition(Vector3 position)
        {
            _params.positionMode = SoundPositionMode.AtPosition;
            _params.position = position;
            return this;
        }

        /// <summary>
        /// Set the sound to follow a transform
        /// </summary>
        public SoundPlayParamsBuilder FollowingTarget(Transform target)
        {
            _params.positionMode = SoundPositionMode.FollowTarget;
            _params.followTarget = target;
            return this;
        }

        /// <summary>
        /// Set volume multiplier
        /// </summary>
        public SoundPlayParamsBuilder WithVolume(float volume)
        {
            _params.volumeMultiplier = volume;
            return this;
        }

        /// <summary>
        /// Set pitch multiplier
        /// </summary>
        public SoundPlayParamsBuilder WithPitch(float pitch)
        {
            _params.pitchMultiplier = pitch;
            return this;
        }

        /// <summary>
        /// Set loop behavior
        /// </summary>
        public SoundPlayParamsBuilder WithLoop(bool shouldLoop)
        {
            _params.loop = shouldLoop;
            return this;
        }

        /// <summary>
        /// Set completion callback
        /// </summary>
        public SoundPlayParamsBuilder OnComplete(Action<SoundHandle> callback)
        {
            _params.onComplete = callback;
            return this;
        }

        /// <summary>
        /// Build the final parameters
        /// </summary>
        public SoundPlayParams Build() => _params;

        /// <summary>
        /// Implicit conversion to SoundPlayParams
        /// </summary>
        public static implicit operator SoundPlayParams(SoundPlayParamsBuilder builder) => builder.Build();
    }
}