# AudioFlux + Ludo.UnityInject Quick Reference

## Setup Checklist

### ✅ 1. Installer Setup
```csharp
[CreateAssetMenu(fileName = "AudioFluxInstaller", menuName = "Galacron/Installers/AudioFluxInstaller")]
public class AudioFluxInstaller : ScriptableObjectInstaller
{
    [SerializeField] private SfxService sfxServicePrefab;
    [SerializeField] private MusicService musicServicePrefab;
    [SerializeField] private AudioFluxModuleManager moduleManagerPrefab;
    
    public override void InstallBindings(IContainer container)
    {
        BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");
        BindAndLog<IMusicService, MusicService>(container, musicServicePrefab, "MusicService");
        BindAndLog<AudioFluxModuleManager, AudioFluxModuleManager>(container, moduleManagerPrefab, "ModuleManager");
    }
}
```

### ✅ 2. Basic Component Usage
```csharp
public class Weapon : MonoBehaviour
{
    [Inject] private ISFXService _sfxService;
    [Inject] private IMusicService _musicService;
    
    public void Fire()
    {
        _sfxService.PlaySoundWithModules("weapon_fire", SoundPlayParams.At(firePoint.position));
        _musicService.SetMusicIntensity(0.8f);
    }
}
```

### ✅ 3. Module with DI
```csharp
[AudioFluxModule("example.module", "Example", "1.0.0")]
public class ExampleModule : SFXServiceModuleBase
{
    [Inject] private IPoolManager _poolManager;
    
    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        // Use injected services in modules
        var effect = _poolManager?.GetPooledObject(effectPrefab);
    }
}
```

## Common Patterns

### 🎯 Service Injection
```csharp
// Basic injection
[Inject] private ISFXService _sfxService;
[Inject] private IMusicService _musicService;

// With null checking
public void PlaySound(string soundId)
{
    _sfxService?.PlaySound(soundId);
}
```

### 🎯 Hybrid Compatibility
```csharp
public class SmartComponent : MonoBehaviour
{
    [Inject] private ISoundManager _legacySound;    // Fallback
    [Inject] private ISFXService _audioFlux;       // Preferred
    
    public void PlaySound()
    {
        if (_audioFlux != null)
            _audioFlux.PlaySound("sound_id");
        else
            _legacySound?.PlayOneShot(clip, SoundType.SFX);
    }
}
```

### 🎯 Runtime Object Injection
```csharp
public class Spawner : MonoBehaviour
{
    [Inject] private IContainer _container;
    
    public GameObject Spawn(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        MonoBehaviourInjector.InjectGameObject(_container, instance);
        return instance;
    }
}
```

### 🎯 Cross-System Integration
```csharp
[AudioFluxModule("analytics.audio", "Audio Analytics", "1.0.0")]
public class AudioAnalyticsModule : SFXServiceModuleBase
{
    [Inject] private IAnalyticsService _analytics;
    [Inject] private IPoolManager _pools;
    
    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        _analytics?.TrackEvent("sound_played", new { soundId });
        var effect = _pools?.GetPooledObject(visualEffectPrefab);
    }
}
```

## Quick Troubleshooting

### ❌ Service is null
**Check**: Installer registration, prefab references, binding order

### ❌ Module dependencies not working
**Solution**: Manual injection in OnInitialize()
```csharp
protected override void OnInitialize()
{
    var container = FindObjectOfType<MonoBehaviourInjector>()?.Container;
    container?.Inject(this);
}
```

### ❌ Runtime objects missing services
**Solution**: Always inject after instantiation
```csharp
var obj = Instantiate(prefab);
MonoBehaviourInjector.InjectGameObject(_container, obj);
```

## Migration Strategy

### Phase 1: Add Services
```csharp
// Add to existing installer alongside legacy audio
BindAndLog<ISoundManager, SoundManager>(container, soundManagerPrefab, "SoundManager");  // Keep
BindAndLog<ISFXService, SfxService>(container, sfxServicePrefab, "SfxService");          // Add
```

### Phase 2: Gradual Component Updates
```csharp
// Update components one by one
[Inject] private ISFXService _sfxService;  // New
// Remove: [SerializeField] private SoundManager soundManager;  // Old
```

### Phase 3: Enable Modules
```csharp
// Add module manager to installer
BindAndLog<AudioFluxModuleManager, AudioFluxModuleManager>(container, moduleManagerPrefab, "ModuleManager");
```

## Testing Patterns

### Mock Services
```csharp
[Test]
public void TestWeaponFire()
{
    var mockSfx = new Mock<ISFXService>();
    var weapon = CreateWeaponWithMocks(mockSfx.Object);
    
    weapon.Fire();
    
    mockSfx.Verify(s => s.PlaySound("weapon_fire", It.IsAny<SoundPlayParams>()), Times.Once);
}
```

### Integration Testing
```csharp
[Test]
public void TestFullAudioPipeline()
{
    // Create real services for integration testing
    var sfxService = CreateRealSfxService();
    var container = CreateTestContainer();
    container.Bind<ISFXService>().FromInstance(sfxService);
    
    var weapon = CreateWeapon();
    container.Inject(weapon);
    
    weapon.Fire();
    
    Assert.IsTrue(sfxService.IsPlaying("weapon_fire"));
}
```

## Performance Tips

### ✅ Cache Service References
```csharp
private bool _servicesInitialized;
private bool _hasSfxService;

private void InitializeServices()
{
    _hasSfxService = _sfxService != null;
    _servicesInitialized = true;
}
```

### ✅ Lazy Resolution
```csharp
private ISFXService _sfxService;
private ISFXService SfxService => _sfxService ??= FindObjectOfType<SfxService>();
```

### ✅ Batch Operations
```csharp
public void PlayMultipleSounds(string[] soundIds)
{
    if (_sfxService == null) return;
    
    foreach (var soundId in soundIds)
    {
        _sfxService.PlaySound(soundId);
    }
}
```

## Best Practices Summary

- ✅ Always use interfaces (`ISFXService`, `IMusicService`)
- ✅ Check for null before using services
- ✅ Inject runtime objects with `MonoBehaviourInjector.InjectGameObject()`
- ✅ Use modules for cross-system integration
- ✅ Test with mocks for unit tests
- ✅ Cache service availability for performance
- ✅ Follow gradual migration strategy
- ✅ Register services in correct order (dependencies first)

## Common Service Interfaces

| Interface | Purpose | Key Methods |
|-----------|---------|-------------|
| `ISFXService` | Sound effects | `PlaySound()`, `PlaySoundWithModules()` |
| `IMusicService` | Music playback | `PlayMusic()`, `SetGameState()`, `SetMusicIntensity()` |
| `IPoolManager` | Object pooling | `GetPooledObject()`, `ReturnToPool()` |
| `IAnalyticsService` | Analytics tracking | `TrackEvent()`, `UpdateGlobalContext()` |
| `ISaveManager` | Save/Load data | `SaveData()`, `LoadData()` |
| `ISceneLoader` | Scene management | `LoadScene()`, `OnSceneLoaded` |

## Extension Methods Available

```csharp
// Enhanced playback
_sfxService.PlaySoundWithModules(soundId, params);

// Game state management
_musicService.SetGameState("combat");
_musicService.TransitionToMusicState("exploration", 2f);
_musicService.SetMusicIntensity(0.8f);

// Module features
handle.RequestOcclusion();
handle.SetImportant(true);
handle.ForceLODLevel(AudioLODLevel.High);

// Status checking
var isOccluded = handle.IsOccluded();
var lodLevel = handle.GetLODLevel();
var currentState = _musicService.GetCurrentMusicState();
```

---

**📚 For complete documentation, see [DependencyInjection.md](DependencyInjection.md)**
