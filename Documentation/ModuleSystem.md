# AudioFlux Module System

The AudioFlux Module System provides a powerful, extensible architecture for adding advanced audio features without breaking existing code. 
The system follows the principle of **zero breaking changes** - all existing AudioFlux APIs remain exactly the same.

## Core Principles

- **Zero Breaking Changes**: Existing APIs remain unchanged
- **Optional Features**: Advanced features only activate when modules are present
- **Composition Over Inheritance**: Features extend through composition, not inheritance
- **Event-Driven Extensions**: Advanced features listen to core events
- **Lazy Loading**: Features initialize only when needed

## Architecture Overview

### Core Components

1. **IAudioFluxModule**: Base interface for all modules
2. **AudioFluxModuleManager**: Manages module discovery and lifecycle
3. **AudioFluxModuleAttribute**: Provides module metadata
4. **Module Base Classes**: Simplified development for different module types
5. **Service Extensions**: Seamless integration with existing services

### Module Categories

- **Advanced3D**: Audio occlusion, reverb zones, directional audio
- **Performance**: Streaming, LOD, memory analytics
- **Physics**: Material-based audio, collision sounds
- **DynamicMusic**: State-based transitions, intensity scaling
- **Interactive**: Stems, beat sync, tempo changes
- **Specialized**: Weapon audio, vehicle systems, dialogue

## Quick Start

### 1. Setup Module Manager

```csharp
// Add AudioFluxModuleManager to your scene
// GameObject → Ludo → AudioFlux → Module Manager

// Or create programmatically:
var moduleManager = gameObject.AddComponent<AudioFluxModuleManager>();
moduleManager.musicService = musicService;
moduleManager.sfxService = sfxService;
```

### 2. Using Existing Modules

```csharp
// Use extension methods for seamless integration
var handle = sfxService.PlaySoundWithModules("explosion", 
    SoundPlayParams.At(position).WithImportance(true));

// Set game state for dynamic music
musicService.SetGameState("combat");

// Change music intensity
musicService.SetMusicIntensity(0.8f);

// Request occlusion calculation
handle.RequestOcclusion();
```

### 3. Check Module Status

```csharp
// Get current music state (if state module is present)
var currentState = musicService.GetCurrentMusicState();

// Check if sound is occluded (if occlusion module is present)
var isOccluded = handle.IsOccluded();

// Get LOD level (if LOD module is present)
var lodLevel = handle.GetLODLevel();
```

## Built-in Modules

### Audio Occlusion Module

Simulates sound being blocked by walls and objects.

```csharp
[AudioFluxModule("advanced3d.occlusion", "Audio Occlusion", "1.0.0")]
public class AudioOcclusionModule : SFXServiceModuleBase
{
    // Automatically applies occlusion to 3D sounds
    // Configurable through module settings
}

// Usage:
handle.RequestOcclusion(); // Manual occlusion request
var occlusionFactor = occlusionModule.GetOcclusionFactor(handle);
```

### State-Based Music Module

Provides smooth music transitions based on game state.

```csharp
[AudioFluxModule("dynamic.statemusic", "State-Based Music", "1.0.0")]
public class StateBasedMusicModule : MusicServiceModuleBase
{
    // Handles state transitions and intensity scaling
}

// Usage:
musicService.TransitionToMusicState("combat", 2f); // 2-second transition
musicService.SetMusicIntensity(0.7f); // 70% intensity
```

### Audio LOD Module

Provides Level of Detail system for better performance.

```csharp
[AudioFluxModule("performance.audiolod", "Audio LOD System", "1.0.0")]
public class AudioLODModule : SFXServiceModuleBase
{
    // Automatically reduces audio quality at distance
}

// Usage:
handle.SetImportant(true); // Better LOD treatment
handle.ForceLODLevel(AudioLODLevel.High); // Override automatic LOD
```

### Surface Material Audio Module

Different audio effects based on surface materials.

```csharp
[AudioFluxModule("physics.surfacematerial", "Surface Material Audio", "1.0.0")]
public class SurfaceMaterialAudioModule : SFXServiceModuleBase
{
    // Detects surface materials and plays appropriate sounds
}

// Usage:
surfaceModule.PlayFootstepSound(playerPosition);
surfaceModule.PlayImpactSound(impactPosition, intensity);
```

## Creating Custom Modules

### 1. Basic Module Structure

```csharp
[AudioFluxModule(
    "mycompany.customfeature", 
    "Custom Feature", 
    "1.0.0",
    Category = "Custom",
    Description = "My custom audio feature",
    Author = "My Company"
)]
public class CustomAudioModule : SFXServiceModuleBase
{
    public override string ModuleId => "mycompany.customfeature";
    public override string ModuleName => "Custom Feature";
    public override Version ModuleVersion => new Version(1, 0, 0);
    
    protected override void OnInitialize()
    {
        // Initialize your module
        LogInfo("Custom module initialized");
    }
    
    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        // React to sounds starting
    }
}
```

### 2. Module Settings

```csharp
public class CustomAudioModule : SFXServiceModuleBase
{
    [ModuleSetting("Feature Enabled", Description = "Enable/disable the feature")]
    public bool FeatureEnabled { get; set; } = true;
    
    [ModuleSetting("Intensity", Description = "Feature intensity", MinValue = 0f, MaxValue = 1f)]
    public float Intensity { get; set; } = 0.5f;
}
```

### 3. Module Dependencies

```csharp
public class AdvancedModule : AudioFluxModuleBase
{
    public override string[] Dependencies => new[] { "advanced3d.occlusion", "performance.audiolod" };
    
    public override bool IsCompatible(Version audioFluxVersion)
    {
        // Require AudioFlux 1.2.0 or higher
        return audioFluxVersion >= new Version(1, 2, 0);
    }
}
```

### 4. Event-Driven Features

```csharp
protected override void OnInitialize()
{
    // Subscribe to advanced events
    AdvancedAudioEvents.OnGameStateChanged += HandleGameStateChanged;
    AdvancedAudioEvents.OnCollisionAudioRequested += HandleCollisionAudio;
}

private void HandleGameStateChanged(string stateName, object stateData)
{
    // React to game state changes
}
```

## Module Types

### SFXServiceModuleBase
For modules that extend sound effects functionality.

### MusicServiceModuleBase
For modules that extend music functionality.

### HybridServiceModuleBase
For modules that extend both SFX and Music services.

### AudioFluxModuleBase
For completely custom modules.

## Best Practices

### 1. Graceful Degradation
```csharp
// Always check if modules are available
var occlusionModule = moduleManager?.GetModule<AudioOcclusionModule>("advanced3d.occlusion");
if (occlusionModule != null)
{
    // Use advanced feature
    occlusionModule.RequestOcclusion(handle);
}
// Fallback: normal audio playback continues
```

### 2. Performance Considerations
```csharp
public class PerformantModule : SFXServiceModuleBase
{
    [ModuleSetting("Update Frequency", Description = "How often to update (Hz)")]
    public float UpdateFrequency { get; set; } = 10f;
    
    private float _lastUpdate;
    
    protected override void OnUpdate()
    {
        if (Time.time - _lastUpdate >= 1f / UpdateFrequency)
        {
            DoExpensiveOperation();
            _lastUpdate = Time.time;
        }
    }
}
```

### 3. Error Handling
```csharp
protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
{
    try
    {
        ProcessSound(handle);
    }
    catch (Exception ex)
    {
        LogError($"Error processing sound {soundId}: {ex.Message}");
        // Continue gracefully
    }
}
```

## Extension Methods

The module system provides extension methods for seamless integration:

```csharp
// Enhanced sound playback
var handle = sfxService.PlaySoundWithModules(soundId, playParams);

// Game state management
musicService.SetGameState("exploration");
musicService.TransitionToMusicState("combat", 2f);

// Audio analysis
var isOccluded = handle.IsOccluded();
var lodLevel = handle.GetLODLevel();
var currentState = musicService.GetCurrentMusicState();
```

## Events System

### Core Module Events
- `ModuleEvents.OnModuleInitialized`
- `ModuleEvents.OnModuleEnabled`
- `ModuleEvents.OnModuleDisabled`
- `ModuleEvents.OnModuleInitializationFailed`

### Advanced Audio Events
- `AdvancedAudioEvents.OnAudioOcclusionRequested`
- `AdvancedAudioEvents.OnGameStateChanged`
- `AdvancedAudioEvents.OnMusicIntensityChanged`
- `AdvancedAudioEvents.OnCollisionAudioRequested`

## Configuration

Modules can be configured through:

1. **Module Settings**: Properties marked with `[ModuleSetting]`
2. **Inspector**: Runtime configuration in the Module Manager
3. **Code**: Direct property access
4. **Events**: Dynamic configuration through events

## Debugging

### Module Status
```csharp
// List all modules
foreach (var module in moduleManager.Modules.Values)
{
    Debug.Log($"{module.ModuleName}: {(module.IsEnabled ? "Enabled" : "Disabled")}");
}

// Check specific module
var module = moduleManager.GetModule<AudioOcclusionModule>("advanced3d.occlusion");
Debug.Log($"Occlusion Module: {(module?.IsEnabled ?? false ? "Available" : "Not Available")}");
```

### Performance Monitoring
```csharp
// Subscribe to module events for monitoring
ModuleEvents.OnModuleInitializationFailed += (module, ex) => 
{
    Debug.LogError($"Module {module.ModuleName} failed: {ex.Message}");
};
```

## Migration Guide

The module system is designed for zero breaking changes:

1. **Existing code continues to work unchanged**
2. **Add modules gradually** - start with one category
3. **Use extension methods** for enhanced functionality
4. **Modules are optional** - missing modules don't break anything

## Future Modules

The architecture supports future expansion:

- **Weapon Audio Framework**: Complete weapon sound system
- **Vehicle Audio System**: Engine, tire, wind sounds
- **Dialogue System Integration**: Voice processing and lip-sync
- **Ambient Soundscape Manager**: Layered environmental audio
- **Audio Streaming**: Support for large music files
- **Interactive Music**: Real-time stem mixing
- **Beat-Synchronized Events**: Advanced timing-based gameplay

## Conclusion

The AudioFlux Module System provides a powerful foundation for extending audio functionality while maintaining backward compatibility. Start with the built-in modules and gradually add custom features as needed.