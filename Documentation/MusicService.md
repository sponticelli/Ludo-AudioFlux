# MusicService Documentation

The MusicService provides comprehensive music playback functionality for Unity games, including crossfading, layered music, beat tracking, and advanced audio management.

## Features

- **Music Library Management**: Organize and play music using MusicDefinition assets
- **Crossfading**: Smooth transitions between different music tracks
- **Layered Music**: Support for multiple audio layers that can be mixed dynamically
- **Beat Tracking**: Synchronize game events with music beats and bars
- **Intro/Loop System**: Support for intro clips that transition to looping main tracks
- **Ducking**: Automatically lower music volume for dialogue or sound effects
- **Advanced Parameters**: Volume, pitch, start time, and callback support
- **Editor Integration**: Runtime testing and debugging tools

## Quick Start

### 1. Create a MusicService

```csharp
// In the Unity Editor:
// GameObject → Ludo → AudioFlux → Music Service
```

### 2. Create MusicDefinition Assets

```csharp
// In the Project window:
// Right-click → Create → Ludo → AudioFlux → Music Definition
```

### 3. Configure MusicDefinitions

Set up your music clips, volume, BPM, layers, and other properties in the MusicDefinition inspector.

### 4. Add to Music Library

Drag your MusicDefinition assets into the MusicService's Music Library array.

### 5. Play Music

```csharp
// Simple playback
musicService.PlayMusic("background_music");

// With parameters
var playParams = new MusicPlayParamsBuilder()
    .WithVolume(0.8f)
    .WithLayers(true)
    .OnBeat(beat => Debug.Log($"Beat: {beat}"))
    .Build();

musicService.PlayMusic("background_music", playParams);
```

## Core Components

### MusicService

The main service class that handles all music playback operations.

**Key Methods:**
- `PlayMusic(string musicId, MusicPlayParams playParams = default)`
- `CrossfadeTo(string musicId, float crossfadeDuration = 1f, MusicPlayParams playParams = default)`
- `StopMusic(float fadeOutTime = 0f)`
- `PauseMusic()` / `ResumeMusic()`
- `SetMusicVolume(float volume)`
- `SetDucking(float duckingLevel, float duckingTime = 0.5f)`

### MusicDefinition

ScriptableObject that defines music properties:
- Main music clip and optional intro clip
- Volume, pitch, and loop settings
- BPM and timing information
- Layer clips for dynamic mixing
- Fade in/out durations
- Audio mixer group assignment

### MusicPlayParams & MusicPlayParamsBuilder

Parameter system for controlling music playback:

```csharp
var playParams = new MusicPlayParamsBuilder()
    .WithVolume(0.9f)
    .WithPitch(1.1f)
    .WithLayers(true)
    .WithLayerVolumes(1f, 0.5f, 0.3f)
    .FromTime(30f)
    .SkipIntro()
    .OnComplete(handle => Debug.Log("Music completed"))
    .OnBeat(beat => TriggerVisualEffect())
    .OnBar(bar => UpdateUI())
    .Build();
```

### PlayMusicComponent

Helper component for easy music playback without code:
- Drag-and-drop music ID selection
- Play on Start/Enable options
- Runtime parameter adjustment
- Built-in crossfade support

## Advanced Features

### Beat Tracking

Synchronize game events with music:

```csharp
var playParams = new MusicPlayParamsBuilder()
    .OnBeat(beat => {
        // Trigger on every beat
        FlashLight();
    })
    .OnBar(bar => {
        // Trigger on every bar
        SpawnEffect();
    })
    .Build();
```

### Layered Music

Create dynamic music that responds to gameplay:

```csharp
// Start with base layer only
var playParams = new MusicPlayParamsBuilder()
    .WithLayerVolumes(1f, 0f, 0f) // Only first layer
    .Build();

musicService.PlayMusic("combat_music", playParams);

// Later, add intensity layers
var handle = musicService.GetCurrentMusic();
if (handle != null)
{
    // Gradually increase layer volumes based on combat intensity
    handle.SetLayerVolume(1, combatIntensity * 0.8f);
    handle.SetLayerVolume(2, combatIntensity * 0.6f);
}
```

### Ducking

Automatically manage music volume for dialogue or important sounds:

```csharp
// Duck music when dialogue starts
musicService.SetDucking(0.3f, 0.5f); // 30% volume over 0.5 seconds

// Return to normal when dialogue ends
musicService.SetDucking(1f, 0.5f); // 100% volume over 0.5 seconds
```

### Crossfading

Smooth transitions between music tracks:

```csharp
// Simple crossfade
musicService.CrossfadeTo("new_music", 2f); // 2-second crossfade

// Crossfade with parameters
var playParams = MusicPlayParams.WithVolume(0.8f);
musicService.CrossfadeTo("new_music", 3f, playParams);
```

## Editor Tools

### MusicService Inspector

The custom editor provides runtime testing capabilities:
- Music ID dropdown with all available music
- Volume, pitch, and parameter controls
- Play, stop, crossfade, pause/resume buttons
- Advanced controls for intro, layers, start time
- Ducking controls
- Real-time debug information

### PlayMusicComponent Inspector

Enhanced editor for the PlayMusicComponent:
- Music ID dropdown populated from scene MusicService
- Parameter adjustment in the inspector
- Runtime testing buttons
- Automatic MusicService detection

## Best Practices

### 1. Organize Your Music

```csharp
// Use clear, descriptive names for music IDs
"menu_main"
"gameplay_exploration"
"gameplay_combat_low"
"gameplay_combat_high"
"cutscene_dramatic"
```

### 2. Use Crossfading for Smooth Transitions

```csharp
// Instead of abrupt stops
musicService.StopMusic();
musicService.PlayMusic("new_music");

// Use crossfading
musicService.CrossfadeTo("new_music", 1.5f);
```

### 3. Leverage Beat Tracking

```csharp
// Synchronize visual effects with music
var playParams = new MusicPlayParamsBuilder()
    .OnBeat(beat => {
        if (beat % 4 == 0) // Every bar
        {
            TriggerStrongVisualEffect();
        }
        else
        {
            TriggerSubtleVisualEffect();
        }
    })
    .Build();
```

### 4. Use Ducking for Better Audio Mix

```csharp
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private MusicService musicService;
    
    public void StartDialogue()
    {
        musicService.SetDucking(0.25f, 0.3f);
    }
    
    public void EndDialogue()
    {
        musicService.SetDucking(1f, 0.5f);
    }
}
```

### 5. Preload Important Music

```csharp
private void Start()
{
    // Preload music that might be needed soon
    musicService.PreloadMusic("combat_music");
    musicService.PreloadMusic("boss_music");
}
```

## Integration with Existing Systems

The MusicService is designed to work alongside the existing SFXService and can be easily integrated into your game's audio architecture. Both services share similar patterns and can be used together for comprehensive audio management.

## Troubleshooting

### Music Not Playing
- Check that MusicDefinition is added to the Music Library
- Verify the music ID matches the MusicDefinition name
- Ensure audio clips are assigned in the MusicDefinition

### Beat Tracking Not Working
- Verify BPM is set correctly in the MusicDefinition
- Check that beat tracking is enabled in the MusicService
- Ensure callbacks are assigned in MusicPlayParams

### Crossfading Issues
- Check that both music tracks have compatible settings
- Verify crossfade duration is appropriate for the music
- Ensure sufficient audio sources are available
