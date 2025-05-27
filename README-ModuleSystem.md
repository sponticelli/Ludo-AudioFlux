# AudioFlux Module System

## Overview

The AudioFlux Module System provides a powerful, extensible architecture for adding advanced audio features while maintaining **zero breaking changes** to existing code. All current AudioFlux APIs remain exactly the same, while new optional modules provide enhanced functionality.

## 🚀 Quick Start

### 1. Add Module Manager to Scene
```csharp
// GameObject → Ludo → AudioFlux → Module Manager
// Or add AudioFluxModuleManager component manually
```

### 2. Use Enhanced Features
```csharp
// Existing code works unchanged:
sfxService.PlaySound("explosion", SoundPlayParams.At(position));

// Enhanced with modules:
var handle = sfxService.PlaySoundWithModules("explosion", 
    SoundPlayParams.At(position).WithImportance(true));

// Check if advanced features are available:
if (handle.IsOccluded()) { /* occlusion module is active */ }
```

### 3. Control Dynamic Music
```csharp
// Set game state for automatic music transitions
musicService.SetGameState("combat");

// Control music intensity
musicService.SetMusicIntensity(0.8f);

// Manual state transitions
musicService.TransitionToMusicState("exploration", 2f);
```

## 📦 Built-in Modules

### 🎯 Audio Occlusion (`advanced3d.occlusion`)
- Simulates sound being blocked by walls/objects
- Automatic raycast-based occlusion detection
- Configurable occlusion layers and distances
- Low-pass filtering for muffled effects

### 🎵 State-Based Music (`dynamic.statemusic`)
- Smooth music transitions based on game state
- Intensity-based layer mixing
- Configurable transition curves
- Support for intro/loop/outro sequences

### ⚡ Audio LOD (`performance.audiolod`)
- Distance-based audio quality reduction
- Automatic culling of distant sounds
- Important sound prioritization
- Configurable quality levels

### 🏗️ Surface Materials (`physics.surfacematerial`)
- Material-based footstep sounds
- Collision audio generation
- Surface-specific reverb effects
- Tag and layer-based material detection

## 🛠️ Creating Custom Modules

### Basic Module Structure
```csharp
[AudioFluxModule(
    "mycompany.feature", 
    "My Feature", 
    "1.0.0",
    Category = "Custom",
    Description = "My custom audio feature"
)]
public class MyAudioModule : SFXServiceModuleBase
{
    [ModuleSetting("Enabled", Description = "Enable this feature")]
    public bool Enabled { get; set; } = true;
    
    protected override void OnSoundStartedInternal(string soundId, SoundHandle handle)
    {
        if (Enabled)
        {
            // Process sound with your custom logic
        }
    }
}
```

### Module Types
- **SFXServiceModuleBase**: Extends sound effects
- **MusicServiceModuleBase**: Extends music functionality  
- **HybridServiceModuleBase**: Extends both SFX and Music
- **AudioFluxModuleBase**: Completely custom modules

## 🎮 Example Usage

```csharp
public class GameAudioManager : MonoBehaviour
{
    [SerializeField] private MusicService musicService;
    [SerializeField] private SfxService sfxService;
    
    public void OnPlayerEnterCombat()
    {
        // Automatic music transition (if state module is present)
        musicService.SetGameState("combat");
        
        // Play combat sound with enhanced features
        var handle = sfxService.PlaySoundWithModules("combat_start", 
            SoundPlayParams.At(player.position).WithImportance(true));
    }
    
    public void OnPlayerFootstep(Vector3 position)
    {
        // Surface-aware footsteps (if surface module is present)
        var surfaceModule = FindObjectOfType<AudioFluxModuleManager>()
            ?.GetModule<SurfaceMaterialAudioModule>("physics.surfacematerial");
        
        if (surfaceModule != null)
        {
            surfaceModule.PlayFootstepSound(position);
        }
        else
        {
            // Fallback to regular footstep
            sfxService.PlaySound("footstep_default", SoundPlayParams.At(position));
        }
    }
}
```

## 🔧 Configuration

### Module Settings
Modules expose configurable settings through the `[ModuleSetting]` attribute:

```csharp
[ModuleSetting("Update Frequency", Description = "How often to update (Hz)", MinValue = 1f, MaxValue = 60f)]
public float UpdateFrequency { get; set; } = 10f;
```

### Runtime Configuration
```csharp
// Enable/disable modules at runtime
moduleManager.EnableModule("advanced3d.occlusion");
moduleManager.DisableModule("performance.audiolod");

// Access module settings
var occlusionModule = moduleManager.GetModule<AudioOcclusionModule>("advanced3d.occlusion");
occlusionModule.MaxOcclusionDistance = 50f;
```

## 📊 Performance

### Optimized Updates
```csharp
[ModuleSetting("Update Frequency")]
public float UpdateFrequency { get; set; } = 10f; // 10 Hz instead of 60 Hz

protected override void OnUpdate()
{
    if (Time.time - lastUpdate >= 1f / UpdateFrequency)
    {
        DoExpensiveCalculation();
        lastUpdate = Time.time;
    }
}
```

### Graceful Degradation
- Missing modules don't break functionality
- Automatic fallback to standard behavior
- Optional features only activate when needed

## 🎯 Future Modules

The architecture supports unlimited expansion:

- **Weapon Audio Framework**: Complete weapon sound system
- **Vehicle Audio System**: Engine, tire, wind sounds  
- **Dialogue System**: Voice processing and lip-sync
- **Ambient Soundscapes**: Layered environmental audio
- **Audio Streaming**: Large file support
- **Interactive Music**: Real-time stem mixing
- **Beat Synchronization**: Advanced timing features

## 🔄 Migration

### Zero Breaking Changes
- All existing AudioFlux code continues to work
- No API changes required
- Gradual adoption possible

### Enhancement Path
1. Add `AudioFluxModuleManager` to scene
2. Use extension methods for enhanced features
3. Add custom modules as needed
4. Configure through inspector or code

## 📚 Documentation

- **[Complete Module System Guide](Documentation/ModuleSystem.md)**
- **[API Reference](Documentation/)**
- **[Example Scripts](Examples/)**

## 🎉 Benefits

✅ **Zero Breaking Changes** - Existing code unchanged  
✅ **Optional Features** - Only active when modules present  
✅ **Easy Extension** - Simple module creation  
✅ **Performance Optimized** - Configurable update rates  
✅ **Editor Integration** - Visual module management  
✅ **Event-Driven** - Loose coupling between systems  
✅ **Future-Proof** - Unlimited expansion potential  

## 🚀 Get Started

1. Add `AudioFluxModuleManager` to your scene
2. Try the built-in modules with your existing audio
3. Create custom modules for your specific needs
4. Enjoy enhanced audio with zero code changes!

The AudioFlux Module System brings professional-grade audio features to your project while maintaining the simplicity and reliability you expect.
