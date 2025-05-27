# Ludo-AudioFlux

A powerful, extensible audio system for Unity3D

## Overview

AudioFlux is a comprehensive audio solution that combines:

- **🎵 Music Service**: Advanced music playback with crossfading, ducking, and layered compositions
- **🔊 SFX Service**: Sophisticated sound effects with 3D positioning, pooling, and completion callbacks
- **🧩 Module System**: Extensible architecture for advanced features like occlusion, LOD, and dynamic music
- **💉 Dependency Injection**: Full compatibility with Ludo.UnityInject for clean, testable code
- **🎯 Zero Breaking Changes**: Existing audio code continues to work unchanged

## Key Features

### Core Audio Services
- **Interface-based design** (`ISFXService`, `IMusicService`) for dependency injection
- **Object pooling** for optimal performance
- **3D spatial audio** with distance attenuation and positioning
- **Fade in/out** and crossfading capabilities
- **Volume management** with global and per-sound control
- **Event system** for audio lifecycle management

### Advanced Module System
- **Audio Occlusion**: Realistic sound blocking by walls and objects
- **State-Based Music**: Dynamic music transitions based on game state
- **Audio LOD**: Distance-based quality reduction for performance
- **Surface Materials**: Material-aware footsteps and collision sounds
- **Custom Modules**: Easy creation of specialized audio features

### Developer Experience
- **Dependency Injection Ready**: Works seamlessly with Ludo.UnityInject
- **Builder Pattern APIs**: Fluent, readable code for complex audio setups
- **Comprehensive Events**: React to audio lifecycle and state changes
- **Editor Integration**: Visual module management and configuration
- **Extensive Documentation**: Complete guides and examples

## Quick Start

### Basic Usage
```csharp
public class Weapon : MonoBehaviour
{
    [Inject] private ISFXService _sfxService;
    [Inject] private IMusicService _musicService;

    public void Fire()
    {
        // Play positioned sound effect
        _sfxService.PlaySound("weapon_fire", SoundPlayParams.At(firePoint.position));

        // Increase music intensity for combat
        _musicService.SetMusicIntensity(0.8f);
    }
}
```

### Enhanced with Modules
```csharp
public void FireWithAdvancedFeatures()
{
    // Play sound with automatic occlusion and LOD
    var handle = _sfxService.PlaySoundWithModules("weapon_fire",
        SoundPlayParams.At(firePoint.position).WithImportance(true));

    // Transition to combat music state
    _musicService.SetGameState("combat");

    // Check if sound is occluded by walls
    if (handle.IsOccluded())
    {
        // Handle muffled audio feedback
    }
}
```

## Installation

1. Add the package to your Unity project
2. Set up dependency injection (see [Dependency Injection Guide](Documentation/DependencyInjection.md))
3. Configure audio services in your installer
4. Start using AudioFlux in your components!

## Documentation

### 📚 Core Guides
- **[Music Service Guide](Documentation/MusicService.md)** - Complete music playback system with crossfading and layered compositions
- **[Sound Service Guide](Documentation/SoundService.md)** - Advanced sound effects with 3D positioning and pooling
- **[Dependency Injection Guide](Documentation/DependencyInjection.md)** - Integration with Ludo.UnityInject for clean, testable code

### 🧩 Module System
- **[Module System Overview](Documentation/ModuleSystem.md)** - Complete guide to the extensible module architecture
- **[Module System Quick Start](README-ModuleSystem.md)** - Get started with advanced audio features
- **[DI Quick Reference](Documentation/DI-QuickReference.md)** - Quick reference for dependency injection patterns

### 🎯 Specialized Topics
- **[Audio Events](Documentation/AudioEvents.md)** - Event system for audio lifecycle management
- **[Builder Patterns](Documentation/BuilderPatterns.md)** - Fluent APIs for complex audio setups
- **[Performance Guide](Documentation/Performance.md)** - Optimization strategies and best practices

### 🔧 Advanced Features
- **Audio Occlusion** - Realistic sound blocking by walls and objects
- **State-Based Music** - Dynamic music transitions based on game state
- **Audio LOD System** - Distance-based quality reduction for performance
- **Surface Materials** - Material-aware footsteps and collision sounds
- **Cross-System Integration** - Works with pools, analytics, save system, and more

### 📖 Examples
- **[Example Scripts](Examples/)** - Complete working examples and usage patterns
- **[Module Examples](Examples/ModuleSystemExample.cs)** - Demonstrates all module system features

## Architecture

AudioFlux follows clean architecture principles:

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Game Code                           │
│  [Inject] ISFXService, IMusicService, AudioFluxModuleManager │
├─────────────────────────────────────────────────────────────┤
│                  AudioFlux Services                         │
│           SfxService, MusicService (MonoBehaviour)          │
├─────────────────────────────────────────────────────────────┤
│                   Module System                             │
│    Occlusion, LOD, DynamicMusic, SurfaceMaterials, etc.    │
├─────────────────────────────────────────────────────────────┤
│                 Unity Audio System                          │
│              AudioSource, AudioClip, AudioMixer            │
└─────────────────────────────────────────────────────────────┘
```

## Key Benefits

- ✅ **Zero Breaking Changes** - Existing audio code continues to work
- ✅ **Dependency Injection Ready** - Clean, testable, maintainable code
- ✅ **Extensible Architecture** - Add custom modules for specialized features
- ✅ **Performance Optimized** - Object pooling, LOD system, configurable updates
- ✅ **Production Ready** - Comprehensive error handling and edge case management
- ✅ **Cross-Platform** - Works on all Unity-supported platforms
- ✅ **Well Documented** - Extensive guides, examples, and API documentation

## License

This project is part of the Ludo framework for Unity development.