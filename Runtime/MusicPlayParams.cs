using System;
using UnityEngine;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Music playback parameters
    /// </summary>
    public struct MusicPlayParams
    {
        public float? volumeMultiplier;
        public float? pitchMultiplier;
        public bool? loop;
        public float? startTime;
        public bool? playIntro;
        public bool? enableLayers;
        public float[] layerVolumes;
        
        // Callbacks
        public Action<MusicHandle> onComplete;
        public Action<MusicHandle> onIntroComplete;
        public Action<int> onBeat;
        public Action<int> onBar;
        
        /// <summary>
        /// Creates default music parameters
        /// </summary>
        public static MusicPlayParams Default => new MusicPlayParams
        {
            playIntro = true,
            enableLayers = true
        };
        
        /// <summary>
        /// Creates parameters with volume multiplier
        /// </summary>
        public static MusicPlayParams WithVolume(float volume) => new MusicPlayParams
        {
            volumeMultiplier = volume,
            playIntro = true,
            enableLayers = true
        };
        
        /// <summary>
        /// Creates parameters starting from a specific time
        /// </summary>
        public static MusicPlayParams FromTime(float time) => new MusicPlayParams
        {
            startTime = time,
            playIntro = false, // Skip intro when starting from specific time
            enableLayers = true
        };
        
        /// <summary>
        /// Creates parameters without intro
        /// </summary>
        public static MusicPlayParams SkipIntro() => new MusicPlayParams
        {
            playIntro = false,
            enableLayers = true
        };
        
        /// <summary>
        /// Creates parameters with beat callback
        /// </summary>
        public static MusicPlayParams WithBeatCallback(Action<int> onBeat) => new MusicPlayParams
        {
            onBeat = onBeat,
            playIntro = true,
            enableLayers = true
        };
    }
    
    /// <summary>
    /// Builder class for creating complex MusicPlayParams
    /// </summary>
    public class MusicPlayParamsBuilder
    {
        private MusicPlayParams _params;
        
        public MusicPlayParamsBuilder()
        {
            _params = MusicPlayParams.Default;
        }
        
        /// <summary>
        /// Set volume multiplier
        /// </summary>
        public MusicPlayParamsBuilder WithVolume(float volume)
        {
            _params.volumeMultiplier = volume;
            return this;
        }
        
        /// <summary>
        /// Set pitch multiplier
        /// </summary>
        public MusicPlayParamsBuilder WithPitch(float pitch)
        {
            _params.pitchMultiplier = pitch;
            return this;
        }
        
        /// <summary>
        /// Set loop behavior
        /// </summary>
        public MusicPlayParamsBuilder WithLoop(bool shouldLoop)
        {
            _params.loop = shouldLoop;
            return this;
        }
        
        /// <summary>
        /// Set start time
        /// </summary>
        public MusicPlayParamsBuilder FromTime(float time)
        {
            _params.startTime = time;
            _params.playIntro = false; // Skip intro when starting from specific time
            return this;
        }
        
        /// <summary>
        /// Skip intro clip
        /// </summary>
        public MusicPlayParamsBuilder SkipIntro()
        {
            _params.playIntro = false;
            return this;
        }
        
        /// <summary>
        /// Enable/disable layers
        /// </summary>
        public MusicPlayParamsBuilder WithLayers(bool enableLayers)
        {
            _params.enableLayers = enableLayers;
            return this;
        }
        
        /// <summary>
        /// Set specific layer volumes
        /// </summary>
        public MusicPlayParamsBuilder WithLayerVolumes(params float[] volumes)
        {
            _params.layerVolumes = volumes;
            _params.enableLayers = true;
            return this;
        }
        
        /// <summary>
        /// Set completion callback
        /// </summary>
        public MusicPlayParamsBuilder OnComplete(Action<MusicHandle> callback)
        {
            _params.onComplete = callback;
            return this;
        }
        
        /// <summary>
        /// Set intro completion callback
        /// </summary>
        public MusicPlayParamsBuilder OnIntroComplete(Action<MusicHandle> callback)
        {
            _params.onIntroComplete = callback;
            return this;
        }
        
        /// <summary>
        /// Set beat callback
        /// </summary>
        public MusicPlayParamsBuilder OnBeat(Action<int> callback)
        {
            _params.onBeat = callback;
            return this;
        }
        
        /// <summary>
        /// Set bar callback
        /// </summary>
        public MusicPlayParamsBuilder OnBar(Action<int> callback)
        {
            _params.onBar = callback;
            return this;
        }
        
        /// <summary>
        /// Build the final parameters
        /// </summary>
        public MusicPlayParams Build() => _params;
        
        /// <summary>
        /// Implicit conversion to MusicPlayParams
        /// </summary>
        public static implicit operator MusicPlayParams(MusicPlayParamsBuilder builder) => builder.Build();
    }
}