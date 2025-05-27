# Using AudioFlux with Ludo.UnityInject

This guide explains how to integrate AudioFlux with the Ludo.UnityInject dependency injection system, providing clean, testable, and maintainable audio code.

## Overview

AudioFlux is designed to work seamlessly with dependency injection patterns. Instead of manually finding or referencing audio services, you can have them automatically injected into your components, following the same patterns used throughout the Galacron project.

## Benefits of Using DI with AudioFlux

- **Reduced Coupling**: Components don't need direct references to audio services
- **Better Testability**: Easy to mock audio services for unit testing
- **Cleaner Architecture**: Follows SOLID principles and separation of concerns
- **Consistent Patterns**: Same DI approach used throughout the project
- **Module Integration**: Modules can inject other services for cross-system features

## Setup with Ludo.UnityInject

### 1. Register AudioFlux Services in Your Installer

Create or modify your installer to register AudioFlux services:

```csharp
using Ludo.AudioFlux;
using Ludo.AudioFlux.Modules;
using Ludo.UnityInject;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioFluxInstaller", menuName = "Galacron/Installers/AudioFluxInstaller")]
public class AudioFluxInstaller : ScriptableObjectInstaller
{
    [Header("AudioFlux Services")]
    [SerializeField] private SfxService sfxServicePrefab;
    [SerializeField] private MusicService musicServicePrefab;
    [SerializeField] private AudioFluxModuleManager moduleManagerPrefab;

    public override void InstallBindings(IContainer container)
    {
        // Bind core AudioFlux services
        BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");
        BindAndLog<IMusicService, MusicService>(container, musicServicePrefab, "MusicService");
        BindAndLog<AudioFluxModuleManager, AudioFluxModuleManager>(container, moduleManagerPrefab, "ModuleManager");
    }

    private void BindAndLog<TInterface, TImplementation>(IContainer container, TImplementation prefab, string serviceName)
        where TInterface : class
        where TImplementation : MonoBehaviour, TInterface
    {
        bool bound = BindPersistentComponent<TInterface, TImplementation>(container, prefab);
        if (!bound)
        {
            Debug.LogError($"[AudioFluxInstaller] {serviceName} binding failed!");
        }
        else
        {
            Debug.Log($"[AudioFluxInstaller] {serviceName} bound successfully");
        }
    }
}
```

### 2. Add to Existing Installer (Gradual Migration)

If you want to add AudioFlux alongside existing audio systems:

```csharp
// In your existing CoreManagersInstaller
public class CoreManagersInstaller : ScriptableObjectInstaller
{
    [Header("Existing Audio")]
    [SerializeField] private SoundManager soundManagerPrefab;
    [SerializeField] private MusicManager musicManagerPrefab;

    [Header("AudioFlux Services")]
    [SerializeField] private SfxService sfxServicePrefab;
    [SerializeField] private MusicService musicServicePrefab;
    [SerializeField] private AudioFluxModuleManager moduleManagerPrefab;

    public override void InstallBindings(IContainer container)
    {
        // Existing services (unchanged)
        BindAndLog<ISoundManager, SoundManager>(container, soundManagerPrefab, "SoundManager");
        BindAndLog<IMusicManager, MusicManager>(container, musicManagerPrefab, "MusicManager");

        // New AudioFlux services (additive)
        BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");
        BindAndLog<IMusicService, MusicService>(container, musicServicePrefab, "MusicService");
        BindAndLog<AudioFluxModuleManager, AudioFluxModuleManager>(container, moduleManagerPrefab, "ModuleManager");
    }
}
```

## Using AudioFlux Services with Dependency Injection

### Basic Service Injection

```csharp
using Ludo.AudioFlux;
using Ludo.UnityInject;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private string fireSound = "weapon_fire";
    [SerializeField] private string reloadSound = "weapon_reload";

    // Inject AudioFlux services
    [Inject] private ISFXService _sfxService;
    [Inject] private IMusicService _musicService;

    public void Fire()
    {
        // Play fire sound with enhanced features
        var handle = _sfxService.PlaySoundWithModules(fireSound,
            SoundPlayParams.At(firePoint.position).WithImportance(true));

        // Increase music intensity during combat
        _musicService.SetMusicIntensity(0.8f);
    }

    public void Reload()
    {
        // Play reload sound
        _sfxService.PlaySound(reloadSound);
    }
}
```

### Advanced Usage with Module Features

```csharp
public class AdvancedWeapon : MonoBehaviour
{
    [Inject] private ISFXService _sfxService;
    [Inject] private IMusicService _musicService;

    [Header("Combat Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask environmentLayers;

    public void Fire()
    {
        // Play positioned sound with occlusion
        var playParams = new SoundPlayParamsBuilder()
            .AtPosition(firePoint.position)
            .WithImportance(true)
            .WithOcclusion(true)
            .Build();

        var handle = _sfxService.PlaySoundWithModules("weapon_fire", playParams);

        // Request occlusion calculation
        handle.RequestOcclusion();

        // Set combat music state
        _musicService.SetGameState("combat");
    }

    public void OnCombatEnd()
    {
        // Return to exploration music
        _musicService.TransitionToMusicState("exploration", 2f);
    }
}
```

### Hybrid Approach (Backward Compatibility)

```csharp
public class SmartWeapon : MonoBehaviour
{
    // Inject both old and new systems for compatibility
    [Inject] private ISoundManager _soundManager;    // Fallback
    [Inject] private ISFXService _sfxService;        // Preferred

    [SerializeField] private SoundData legacySoundData;
    [SerializeField] private string audioFluxSoundId = "weapon_fire";

    public void Fire()
    {
        // Use AudioFlux if available, fallback to legacy system
        if (_sfxService != null)
        {
            var handle = _sfxService.PlaySoundWithModules(audioFluxSoundId,
                SoundPlayParams.At(firePoint.position));
        }
        else if (_soundManager != null)
        {
            _soundManager.PlayOneShot(legacySoundData, firePoint.position);
        }
    }
}
```

## Module System Integration

### Modules with Dependency Injection

Modules can inject other services for cross-system integration:

```csharp
using Ludo.AudioFlux.Modules;
using Ludo.Core.Pools.Runtime;
using Ludo.UnityInject;

[AudioFluxModule("galacron.weapon", "Weapon Audio Effects", "1.0.0")]
public class WeaponAudioModule : SFXServiceModuleBase
{
    // Inject other services into modules
    [Inject] private IPoolManager _poolManager;

    [Header("Effect Settings")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject shellCasingPrefab;

    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        if (soundId.StartsWith("weapon_fire"))
        {
            CreateMuzzleFlash(handle.audioSource.transform.position);
            CreateShellCasing(handle.audioSource.transform.position);
        }
    }

    private void CreateMuzzleFlash(Vector3 position)
    {
        if (_poolManager != null && muzzleFlashPrefab != null)
        {
            var effect = _poolManager.GetPooledObject(muzzleFlashPrefab);
            effect.transform.position = position;
        }
    }

    private void CreateShellCasing(Vector3 position)
    {
        if (_poolManager != null && shellCasingPrefab != null)
        {
            var casing = _poolManager.GetPooledObject(shellCasingPrefab);
            casing.transform.position = position + Vector3.up * 0.1f;
        }
    }
}
```

### Runtime Object Injection

When creating objects at runtime, ensure they get proper dependency injection:

```csharp
public class WeaponManager : MonoBehaviour
{
    [Inject] private IContainer _container;
    [Inject] private ISFXService _sfxService;

    public GameObject SpawnWeapon(GameObject weaponPrefab, Vector3 position)
    {
        // Instantiate weapon
        GameObject weapon = Instantiate(weaponPrefab, position, Quaternion.identity);

        // Inject dependencies (including AudioFlux services)
        MonoBehaviourInjector.InjectGameObject(_container, weapon);

        // Play spawn sound
        _sfxService.PlaySound("weapon_spawn", SoundPlayParams.At(position));

        return weapon;
    }
}
```

## Testing with Dependency Injection

### Mocking AudioFlux Services

```csharp
using NUnit.Framework;
using Moq;
using Ludo.AudioFlux;

[TestFixture]
public class WeaponTests
{
    private Mock<ISFXService> _mockSfxService;
    private Mock<IMusicService> _mockMusicService;
    private Weapon _weapon;

    [SetUp]
    public void Setup()
    {
        _mockSfxService = new Mock<ISFXService>();
        _mockMusicService = new Mock<IMusicService>();

        // Create weapon and inject mocks
        var weaponObject = new GameObject();
        _weapon = weaponObject.AddComponent<Weapon>();

        // Manual injection for testing
        var sfxField = typeof(Weapon).GetField("_sfxService", BindingFlags.NonPublic | BindingFlags.Instance);
        sfxField.SetValue(_weapon, _mockSfxService.Object);

        var musicField = typeof(Weapon).GetField("_musicService", BindingFlags.NonPublic | BindingFlags.Instance);
        musicField.SetValue(_weapon, _mockMusicService.Object);
    }

    [Test]
    public void Fire_ShouldPlayFireSound()
    {
        // Act
        _weapon.Fire();

        // Assert
        _mockSfxService.Verify(s => s.PlaySoundWithModules("weapon_fire", It.IsAny<SoundPlayParams>()), Times.Once);
        _mockMusicService.Verify(m => m.SetMusicIntensity(0.8f), Times.Once);
    }
}
```

## Best Practices

### 1. Use Interfaces for Testability

```csharp
// Good: Use interfaces for dependency injection
[Inject] private ISFXService _sfxService;
[Inject] private IMusicService _musicService;

// Avoid: Direct references to concrete classes
// [SerializeField] private SfxService sfxService;
```

### 2. Graceful Degradation

```csharp
public void PlaySound(string soundId)
{
    // Always check for null in case service isn't available
    if (_sfxService != null)
    {
        _sfxService.PlaySound(soundId);
    }
    else
    {
        Debug.LogWarning($"SFX Service not available, cannot play sound: {soundId}");
    }
}
```

### 3. Module Service Dependencies

```csharp
[AudioFluxModule("example.module", "Example Module", "1.0.0")]
public class ExampleModule : SFXServiceModuleBase
{
    [Inject] private IPoolManager _poolManager;
    [Inject] private IAnalyticsService _analyticsService;

    protected override void OnInitialize()
    {
        // Verify dependencies are available
        if (_poolManager == null)
        {
            LogWarning("Pool Manager not available - some features will be disabled");
        }

        if (_analyticsService == null)
        {
            LogWarning("Analytics Service not available - no audio analytics");
        }
    }
}
```

### 4. Service Registration Order

```csharp
public override void InstallBindings(IContainer container)
{
    // Register dependencies first
    BindAndLog<IPoolManager, PoolManager>(container, poolManagerPrefab, "PoolManager");

    // Then register AudioFlux services
    BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");
    BindAndLog<IMusicService, MusicService>(container, musicServicePrefab, "MusicService");

    // Finally register module manager (depends on audio services)
    BindAndLog<AudioFluxModuleManager, AudioFluxModuleManager>(container, moduleManagerPrefab, "ModuleManager");
}
```

## Migration Strategy

### Phase 1: Add AudioFlux Services
1. Add AudioFlux services to your installer
2. Keep existing audio system running
3. Test that both systems work together

### Phase 2: Gradual Component Migration
1. Update new components to use AudioFlux
2. Migrate existing components one by one
3. Use hybrid approach during transition

### Phase 3: Module Integration
1. Add module manager to installer
2. Create custom modules for your game
3. Enable advanced audio features

### Phase 4: Legacy Cleanup (Optional)
1. Remove old audio system dependencies
2. Clean up unused audio components
3. Fully migrate to AudioFlux

## Advanced Scenarios

### Cross-System Integration Examples

#### Analytics Integration
```csharp
[AudioFluxModule("analytics.audio", "Audio Analytics", "1.0.0")]
public class AudioAnalyticsModule : HybridServiceModuleBase
{
    [Inject] private IAnalyticsService _analyticsService;

    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        _analyticsService?.TrackEvent("audio_sound_played", new { soundId, position = handle.audioSource.transform.position });
    }

    protected override void OnMusicStartedInternal(string musicId, MusicHandle handle)
    {
        _analyticsService?.TrackEvent("audio_music_started", new { musicId });
    }
}
```

#### Save System Integration
```csharp
public class AudioSettingsManager : MonoBehaviour
{
    [Inject] private ISFXService _sfxService;
    [Inject] private IMusicService _musicService;
    [Inject] private ISaveManager _saveManager;

    private void Start()
    {
        LoadAudioSettings();
    }

    private void LoadAudioSettings()
    {
        var settings = _saveManager.LoadData<AudioSettings>("audio_settings");
        if (settings != null)
        {
            _sfxService.SetGlobalVolume(settings.sfxVolume);
            _musicService.SetGlobalVolume(settings.musicVolume);
        }
    }

    public void SaveAudioSettings()
    {
        var settings = new AudioSettings
        {
            sfxVolume = _sfxService.GetGlobalVolume(),
            musicVolume = _musicService.GetGlobalVolume()
        };
        _saveManager.SaveData("audio_settings", settings);
    }
}
```

### Scene Management Integration

```csharp
public class SceneAudioManager : MonoBehaviour
{
    [Inject] private IMusicService _musicService;
    [Inject] private ISceneLoader _sceneLoader;

    [Header("Scene Music Mapping")]
    [SerializeField] private SceneMusicMapping[] sceneMusicMappings;

    private void Start()
    {
        _sceneLoader.OnSceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName)
    {
        var mapping = Array.Find(sceneMusicMappings, m => m.sceneName == sceneName);
        if (mapping != null)
        {
            _musicService.SetGameState(mapping.musicState);
            _musicService.TransitionToMusicState(mapping.musicState, mapping.transitionTime);
        }
    }
}

[System.Serializable]
public class SceneMusicMapping
{
    public string sceneName;
    public string musicState;
    public float transitionTime = 2f;
}
```

## Troubleshooting

### Common Issues and Solutions

#### 1. Services Not Injected
**Problem**: AudioFlux services are null after injection
**Solution**:
```csharp
// Check installer registration
public override void InstallBindings(IContainer container)
{
    // Ensure services are properly bound
    Debug.Log("Binding AudioFlux services...");
    BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");

    // Verify prefab references are set in inspector
    if (sfxServicePrefab == null)
    {
        Debug.LogError("SfxService prefab is null in installer!");
    }
}
```

#### 2. Module Dependencies Not Resolved
**Problem**: Modules can't access injected services
**Solution**:
```csharp
[AudioFluxModule("example.module", "Example", "1.0.0")]
public class ExampleModule : SFXServiceModuleBase
{
    [Inject] private IPoolManager _poolManager;

    protected override void OnInitialize()
    {
        // Manually inject if needed
        if (_poolManager == null)
        {
            var container = FindObjectOfType<MonoBehaviourInjector>()?.Container;
            if (container != null)
            {
                container.Inject(this);
            }
        }
    }
}
```

#### 3. Runtime Object Injection
**Problem**: Dynamically created objects don't have services injected
**Solution**:
```csharp
public class DynamicObjectSpawner : MonoBehaviour
{
    [Inject] private IContainer _container;

    public GameObject SpawnObject(GameObject prefab)
    {
        var instance = Instantiate(prefab);

        // Always inject dependencies for runtime objects
        MonoBehaviourInjector.InjectGameObject(_container, instance);

        return instance;
    }
}
```

### Performance Considerations

#### 1. Service Caching
```csharp
public class OptimizedAudioComponent : MonoBehaviour
{
    [Inject] private ISFXService _sfxService;

    // Cache frequently used data
    private bool _servicesInitialized;
    private bool _hasValidSfxService;

    private void Start()
    {
        InitializeServices();
    }

    private void InitializeServices()
    {
        _hasValidSfxService = _sfxService != null;
        _servicesInitialized = true;
    }

    public void PlaySound(string soundId)
    {
        if (!_servicesInitialized) InitializeServices();

        if (_hasValidSfxService)
        {
            _sfxService.PlaySound(soundId);
        }
    }
}
```

#### 2. Lazy Service Resolution
```csharp
public class LazyAudioComponent : MonoBehaviour
{
    private ISFXService _sfxService;
    private bool _serviceResolved;

    private ISFXService SfxService
    {
        get
        {
            if (!_serviceResolved)
            {
                _sfxService = FindObjectOfType<SfxService>();
                _serviceResolved = true;
            }
            return _sfxService;
        }
    }

    public void PlaySound(string soundId)
    {
        SfxService?.PlaySound(soundId);
    }
}
```

## Conclusion

AudioFlux integrates seamlessly with Ludo.UnityInject, providing:

- **Clean Architecture**: Interface-based design with proper separation of concerns
- **Easy Testing**: Mockable services for unit testing
- **Gradual Migration**: Can be adopted alongside existing systems
- **Enhanced Features**: Advanced audio capabilities with module system
- **Consistent Patterns**: Same DI approach used throughout the project
- **Cross-System Integration**: Works with pools, analytics, save system, and more
- **Production Ready**: Robust error handling and performance considerations

The combination of AudioFlux and dependency injection provides a robust, maintainable, and feature-rich audio system that scales with your Unity projects.
