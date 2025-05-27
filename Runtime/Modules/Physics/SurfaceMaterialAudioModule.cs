using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludo.AudioFlux.Modules.Physics
{
    /// <summary>
    /// Module that provides surface material-based audio effects
    /// </summary>
    [AudioFluxModule(
        "physics.surfacematerial", 
        "Surface Material Audio", 
        "1.0.0",
        Category = "Physics",
        Description = "Different audio effects based on surface materials",
        Author = "LiteNinja"
    )]
    public class SurfaceMaterialAudioModule : SFXServiceModuleBase
    {
        [ModuleSetting("Default Material", Description = "Default surface material when none is detected")]
        public string DefaultMaterial { get; set; } = "concrete";
        
        [ModuleSetting("Material Detection Range", Description = "Range for surface material detection")]
        public float MaterialDetectionRange { get; set; } = 2f;
        
        [ModuleSetting("Auto Detect Materials", Description = "Automatically detect materials for footsteps")]
        public bool AutoDetectMaterials { get; set; } = true;
        
        public override string ModuleId => "physics.surfacematerial";
        public override string ModuleName => "Surface Material Audio";
        public override Version ModuleVersion => new Version(1, 0, 0);
        
        private readonly Dictionary<string, SurfaceMaterial> _materials = new Dictionary<string, SurfaceMaterial>();
        private readonly Dictionary<string, string> _tagToMaterial = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _layerToMaterial = new Dictionary<string, string>();
        
        [Serializable]
        public class SurfaceMaterial
        {
            public string materialId;
            public string displayName;
            
            [Header("Footstep Sounds")]
            public string[] footstepSoundIds = new string[0];
            
            [Header("Impact Sounds")]
            public string[] impactSoundIds = new string[0];
            
            [Header("Audio Properties")]
            [Range(0f, 2f)] public float volumeMultiplier = 1f;
            [Range(0f, 2f)] public float pitchMultiplier = 1f;
            public float reverbAmount = 0f;
            
            [Header("Physics Properties")]
            public float hardness = 1f; // 0 = soft, 1 = hard
            public float density = 1f;   // affects impact volume
            
            [Header("Reverb Settings")]
            public AudioReverbPreset reverbPreset = AudioReverbPreset.Off;
            public float reverbMix = 0f;
        }
        
        protected override void OnInitialize()
        {
            // Subscribe to surface material detection events
            AdvancedAudioEvents.OnSurfaceMaterialDetected += HandleSurfaceMaterialDetected;
            AdvancedAudioEvents.OnCollisionAudioRequested += HandleCollisionAudio;
            
            // Initialize default materials
            InitializeDefaultMaterials();
            
            LogInfo("Surface Material Audio Module initialized");
        }
        
        protected override void OnDestroy()
        {
            AdvancedAudioEvents.OnSurfaceMaterialDetected -= HandleSurfaceMaterialDetected;
            AdvancedAudioEvents.OnCollisionAudioRequested -= HandleCollisionAudio;
            base.OnDestroy();
        }
        
        private void InitializeDefaultMaterials()
        {
            // Register common surface materials
            RegisterMaterial(new SurfaceMaterial
            {
                materialId = "concrete",
                displayName = "Concrete",
                footstepSoundIds = new[] { "footstep_concrete_01", "footstep_concrete_02", "footstep_concrete_03" },
                impactSoundIds = new[] { "impact_concrete_01", "impact_concrete_02" },
                volumeMultiplier = 1f,
                pitchMultiplier = 1f,
                hardness = 0.9f,
                density = 1f,
                reverbAmount = 0.2f
            });
            
            RegisterMaterial(new SurfaceMaterial
            {
                materialId = "metal",
                displayName = "Metal",
                footstepSoundIds = new[] { "footstep_metal_01", "footstep_metal_02", "footstep_metal_03" },
                impactSoundIds = new[] { "impact_metal_01", "impact_metal_02" },
                volumeMultiplier = 1.2f,
                pitchMultiplier = 1.1f,
                hardness = 1f,
                density = 1.2f,
                reverbAmount = 0.4f,
                reverbPreset = AudioReverbPreset.Hallway
            });
            
            RegisterMaterial(new SurfaceMaterial
            {
                materialId = "wood",
                displayName = "Wood",
                footstepSoundIds = new[] { "footstep_wood_01", "footstep_wood_02", "footstep_wood_03" },
                impactSoundIds = new[] { "impact_wood_01", "impact_wood_02" },
                volumeMultiplier = 0.8f,
                pitchMultiplier = 0.9f,
                hardness = 0.6f,
                density = 0.8f,
                reverbAmount = 0.1f
            });
            
            RegisterMaterial(new SurfaceMaterial
            {
                materialId = "grass",
                displayName = "Grass",
                footstepSoundIds = new[] { "footstep_grass_01", "footstep_grass_02", "footstep_grass_03" },
                impactSoundIds = new[] { "impact_soft_01", "impact_soft_02" },
                volumeMultiplier = 0.6f,
                pitchMultiplier = 0.8f,
                hardness = 0.2f,
                density = 0.3f,
                reverbAmount = 0f
            });
            
            RegisterMaterial(new SurfaceMaterial
            {
                materialId = "water",
                displayName = "Water",
                footstepSoundIds = new[] { "footstep_water_01", "footstep_water_02", "footstep_water_03" },
                impactSoundIds = new[] { "impact_water_01", "impact_water_02" },
                volumeMultiplier = 0.7f,
                pitchMultiplier = 0.9f,
                hardness = 0.1f,
                density = 0.5f,
                reverbAmount = 0.3f
            });
            
            // Map common tags to materials
            MapTagToMaterial("Concrete", "concrete");
            MapTagToMaterial("Metal", "metal");
            MapTagToMaterial("Wood", "wood");
            MapTagToMaterial("Grass", "grass");
            MapTagToMaterial("Water", "water");
            MapTagToMaterial("Ground", "concrete");
            MapTagToMaterial("Floor", "concrete");
        }
        
        /// <summary>
        /// Register a surface material
        /// </summary>
        public void RegisterMaterial(SurfaceMaterial material)
        {
            if (material == null || string.IsNullOrEmpty(material.materialId))
                return;
            
            _materials[material.materialId] = material;
            LogInfo($"Registered surface material: {material.materialId}");
        }
        
        /// <summary>
        /// Map a GameObject tag to a material
        /// </summary>
        public void MapTagToMaterial(string tag, string materialId)
        {
            _tagToMaterial[tag] = materialId;
        }
        
        /// <summary>
        /// Map a physics layer to a material
        /// </summary>
        public void MapLayerToMaterial(int layer, string materialId)
        {
            _layerToMaterial[LayerMask.LayerToName(layer)] = materialId;
        }
        
        /// <summary>
        /// Play a footstep sound based on surface material
        /// </summary>
        public SoundHandle PlayFootstepSound(Vector3 position, string materialId = null)
        {
            var detectedMaterial = materialId ?? DetectSurfaceMaterial(position);
            var material = GetMaterial(detectedMaterial);
            
            if (material.footstepSoundIds.Length == 0)
                return null;
            
            var soundId = material.footstepSoundIds[UnityEngine.Random.Range(0, material.footstepSoundIds.Length)];
            
            var playParams = new SoundPlayParamsBuilder()
                .AtPosition(position)
                .WithVolume(material.volumeMultiplier)
                .WithPitch(material.pitchMultiplier)
                .Build();
            
            var handle = SfxService.PlaySound(soundId, playParams);
            
            // Apply material-specific effects
            if (handle?.audioSource != null)
            {
                ApplyMaterialEffects(handle.audioSource, material);
            }
            
            return handle;
        }
        
        /// <summary>
        /// Play an impact sound based on surface material and collision intensity
        /// </summary>
        public SoundHandle PlayImpactSound(Vector3 position, float intensity, string materialId = null)
        {
            var detectedMaterial = materialId ?? DetectSurfaceMaterial(position);
            var material = GetMaterial(detectedMaterial);
            
            if (material.impactSoundIds.Length == 0)
                return null;
            
            var soundId = material.impactSoundIds[UnityEngine.Random.Range(0, material.impactSoundIds.Length)];
            
            // Scale volume and pitch based on intensity and material properties
            var volumeScale = Mathf.Lerp(0.3f, 1f, intensity) * material.volumeMultiplier * material.density;
            var pitchScale = Mathf.Lerp(0.8f, 1.2f, intensity * material.hardness) * material.pitchMultiplier;
            
            var playParams = new SoundPlayParamsBuilder()
                .AtPosition(position)
                .WithVolume(volumeScale)
                .WithPitch(pitchScale)
                .Build();
            
            var handle = SfxService.PlaySound(soundId, playParams);
            
            // Apply material-specific effects
            if (handle?.audioSource != null)
            {
                ApplyMaterialEffects(handle.audioSource, material);
            }
            
            return handle;
        }
        
        /// <summary>
        /// Detect surface material at a specific position
        /// </summary>
        public string DetectSurfaceMaterial(Vector3 position)
        {
            // Raycast downward to detect surface
            if (UnityEngine.Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, MaterialDetectionRange))
            {
                // Check for material by tag first
                if (!string.IsNullOrEmpty(hit.collider.tag) && _tagToMaterial.TryGetValue(hit.collider.tag, out var tagMaterial))
                {
                    return tagMaterial;
                }
                
                // Check for material by layer
                var layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
                if (_layerToMaterial.TryGetValue(layerName, out var layerMaterial))
                {
                    return layerMaterial;
                }
                
                // Check for SurfaceMaterialComponent
                var materialComponent = hit.collider.GetComponent<SurfaceMaterialComponent>();
                if (materialComponent != null && !string.IsNullOrEmpty(materialComponent.MaterialId))
                {
                    return materialComponent.MaterialId;
                }
            }
            
            return DefaultMaterial;
        }
        
        private SurfaceMaterial GetMaterial(string materialId)
        {
            return _materials.GetValueOrDefault(materialId) ?? _materials.GetValueOrDefault(DefaultMaterial) ?? new SurfaceMaterial { materialId = "default" };
        }
        
        private void ApplyMaterialEffects(AudioSource audioSource, SurfaceMaterial material)
        {
            // Apply reverb if specified
            if (material.reverbAmount > 0f || material.reverbPreset != AudioReverbPreset.Off)
            {
                var reverbFilter = audioSource.GetComponent<AudioReverbFilter>();
                if (reverbFilter == null)
                {
                    reverbFilter = audioSource.gameObject.AddComponent<AudioReverbFilter>();
                }
                
                reverbFilter.reverbPreset = material.reverbPreset;
                reverbFilter.dryLevel = Mathf.Lerp(0f, -1000f, material.reverbMix);
                reverbFilter.reverbLevel = Mathf.Lerp(-1000f, 0f, material.reverbAmount);
            }
        }
        
        private void HandleSurfaceMaterialDetected(Vector3 position, RaycastHit hit)
        {
            // This event can be used by other systems to trigger material-based effects
            var materialId = DetectSurfaceMaterial(position);
            LogInfo($"Surface material detected at {position}: {materialId}");
        }
        
        private void HandleCollisionAudio(Collision collision, float intensity)
        {
            if (collision.contacts.Length > 0)
            {
                var contactPoint = collision.contacts[0].point;
                PlayImpactSound(contactPoint, intensity);
            }
        }
        
        /// <summary>
        /// Get all registered materials
        /// </summary>
        public IReadOnlyDictionary<string, SurfaceMaterial> GetAllMaterials()
        {
            return _materials;
        }
        
        /// <summary>
        /// Get a specific material by ID
        /// </summary>
        public SurfaceMaterial GetMaterialById(string materialId)
        {
            return _materials.GetValueOrDefault(materialId);
        }
    }
    
    /// <summary>
    /// Component that can be attached to GameObjects to specify their surface material
    /// </summary>
    public class SurfaceMaterialComponent : MonoBehaviour
    {
        [SerializeField] private string materialId = "concrete";
        
        public string MaterialId
        {
            get => materialId;
            set => materialId = value;
        }
    }
}