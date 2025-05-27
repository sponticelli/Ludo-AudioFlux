# SoundService (SFXService) Documentation

The SoundService (SfxService) provides comprehensive sound effect management for Unity games, including 3D spatial audio, object pooling, fade effects, and advanced parameter control.

## Features

- **Sound Library Management**: Organize and play sounds using SoundDefinition assets
- **Audio Source Pooling**: Efficient memory management with configurable pool sizes
- **3D Spatial Audio**: Full support for positioned and following sounds
- **Fade Effects**: Smooth fade in/out transitions
- **Random Variation**: Multiple clips per sound with pitch variance
- **Type-Safe Parameters**: Prevent common audio programming errors
- **Advanced Controls**: Volume, pitch, looping, and completion callbacks
- **Editor Integration**: Runtime testing and debugging tools

## Quick Start

### 1. Create a SoundService

```csharp
// In the Unity Editor:
// GameObject → Ludo → AudioFlux → SFX Service
```

### 2. Create SoundDefinition Assets

```csharp
// In the Project window:
// Right-click → Create → Ludo → AudioFlux → Sound Definition
```

### 3. Configure SoundDefinitions

Set up your audio clips, volume, pitch variance, 3D settings, and other properties in the SoundDefinition inspector.

### 4. Add to Sound Library

Drag your SoundDefinition assets into the SfxService's Sound Library array.

### 5. Play Sounds

```csharp
// Simple playback
sfxService.PlaySound("footstep");

// With parameters
var playParams = new SoundPlayParamsBuilder()
    .AtPosition(transform.position)
    .WithVolume(0.8f)
    .WithPitch(1.2f)
    .OnComplete(handle => Debug.Log($"Sound {handle.soundId} completed!"))
    .Build();

sfxService.PlaySound("explosion", playParams);
```

## Core Components

### SfxService

The main service class that handles all sound effect operations.

**Key Methods:**
- `PlaySound(string soundId, SoundPlayParams playParams = default)`
- `PlaySound(SoundDefinition soundDef, SoundPlayParams playParams = default)`
- `StopSound(SoundHandle handle, float fadeOutTime = 0f)`
- `StopAllSounds(float fadeOutTime = 0f)`
- `SetGlobalVolume(float volume)`
- `PauseAll()` / `ResumeAll()`
- `IsPlaying(string soundId)`

### SoundDefinition

ScriptableObject that defines sound properties:
- Multiple audio clips for variation
- Volume and pitch settings with variance
- 3D spatial audio configuration
- Fade in/out durations
- Loop settings
- Audio mixer group assignment
- Priority levels

### SoundPlayParams & SoundPlayParamsBuilder

Type-safe parameter system for controlling sound playback:

```csharp
var playParams = new SoundPlayParamsBuilder()
    .AtPosition(Vector3.zero)           // 3D positioned
    .FollowingTarget(player.transform)  // Follow object
    .WithVolume(0.8f)                   // Volume multiplier
    .WithPitch(1.2f)                    // Pitch multiplier
    .WithLoop(true)                     // Loop the sound
    .OnComplete(handle => {             // Completion callback
        Debug.Log("Sound finished!");
    })
    .Build();
```

### SoundHandle

Handle for controlling individual sound instances:
- `soundId`: Identifier of the playing sound
- `audioSource`: Reference to the AudioSource
- `isValid`: Whether the sound is still valid
- `isPlaying`: Whether the sound is currently playing
- `time`: Current playback time

## Advanced Features

### 3D Spatial Audio

Create immersive 3D soundscapes:

```csharp
// Play sound at specific position
var playParams = SoundPlayParams.At(enemyPosition);
sfxService.PlaySound("enemy_growl", playParams);

// Make sound follow a moving object
var playParams = SoundPlayParams.Following(vehicle.transform);
sfxService.PlaySound("engine_loop", playParams);
```

### Audio Source Pooling

Efficient memory management with automatic pooling:

```csharp
[Header("Pool Settings")]
[SerializeField] private int initialPoolSize = 10;  // Starting pool size
[SerializeField] private int maxPoolSize = 50;      // Maximum pool size
```

The service automatically:
- Creates audio sources as needed
- Reuses finished audio sources
- Manages pool size dynamically
- Cleans up completed sounds

### Sound Variation

Add variety to your audio with multiple clips and pitch variance:

```csharp
// SoundDefinition supports:
// - Multiple AudioClips (randomly selected)
// - Pitch variance for natural variation
// - Volume settings per sound type

// Example: Footstep sounds with 5 different clips
// and ±0.1 pitch variance for natural variation
```

### Fade Effects

Smooth audio transitions:

```csharp
// Fade in over 2 seconds (set in SoundDefinition)
fadeInDuration = 2f;

// Fade out when stopping
sfxService.StopSound(handle, 1.5f); // 1.5 second fade out
sfxService.StopAllSounds(2f);       // Fade out all sounds
```

### Type-Safe Positioning

Prevent common audio positioning errors:

```csharp
// OLD WAY (error-prone):
// var params = new SoundPlayParams {
//     position = Vector3.zero,
//     followTarget = someTransform  // CONFLICT!
// };

// NEW WAY (type-safe):
var positionParams = SoundPlayParams.At(Vector3.zero);        // Position mode
var followParams = SoundPlayParams.Following(someTransform);  // Follow mode

// Can't accidentally mix positioning modes!
```

## Editor Tools

### SfxService Inspector

The custom editor provides comprehensive runtime testing:
- **Sound ID Selection**: Dropdown with all available sounds
- **Manual/Dropdown Toggle**: Flexibility for testing custom IDs
- **Parameter Controls**: Volume, pitch, and position sliders
- **Playback Controls**: Play, stop all, pause/resume buttons
- **Debug Information**: Real-time playback status
- **Help Messages**: Guidance for proper setup

### Sound Testing Workflow

```csharp
// In the editor during play mode:
1. Select sound from dropdown
2. Adjust volume, pitch, position
3. Click "Play Sound" to test
4. Use "Stop All" to clear all sounds
5. Monitor debug info for active sounds
```

## Best Practices

### 1. Organize Your Sounds

```csharp
// Use clear, descriptive names for sound IDs
"ui_button_click"
"player_footstep_grass"
"weapon_pistol_fire"
"ambient_wind_forest"
"enemy_zombie_growl"
```

### 2. Use 3D Audio Effectively

```csharp
// For environmental sounds
var playParams = SoundPlayParams.At(firePosition);
sfxService.PlaySound("fire_crackling", playParams);

// For moving objects
var playParams = SoundPlayParams.Following(car.transform);
var handle = sfxService.PlaySound("car_engine", playParams);
```

### 3. Leverage Sound Variation

```csharp
// In SoundDefinition:
// - Add 3-5 similar clips for footsteps
// - Set pitch variance to ±0.1 for natural variation
// - Use different clips for different materials

// The service automatically handles variation
sfxService.PlaySound("footstep"); // Plays random clip with pitch variance
```

### 4. Use Completion Callbacks

```csharp
var playParams = new SoundPlayParamsBuilder()
    .OnComplete(handle => {
        // Trigger next action when sound finishes
        TriggerExplosionEffect();
        SpawnDebris();
    })
    .Build();

sfxService.PlaySound("bomb_explosion", playParams);
```

### 5. Manage Global Volume

```csharp
public class AudioSettings : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;

    public void SetSFXVolume(float volume)
    {
        sfxService.SetGlobalVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
```

### 6. Preload Important Sounds

```csharp
private void Start()
{
    // Preload sounds that need immediate playback
    sfxService.PreloadSound("ui_button_click");
    sfxService.PreloadSound("weapon_fire");
    sfxService.PreloadSound("player_hurt");
}
```

## Integration Patterns

### UI Sound Manager

```csharp
public class UISoundManager : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;

    public void PlayButtonClick() => sfxService.PlaySound("ui_button_click");
    public void PlayButtonHover() => sfxService.PlaySound("ui_button_hover");
    public void PlayMenuOpen() => sfxService.PlaySound("ui_menu_open");
    public void PlayMenuClose() => sfxService.PlaySound("ui_menu_close");
}
```

### Weapon Sound System

```csharp
public class WeaponAudio : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;
    [SerializeField] private Transform muzzlePoint;

    public void PlayFireSound()
    {
        var playParams = SoundPlayParams.At(muzzlePoint.position);
        sfxService.PlaySound("weapon_fire", playParams);
    }

    public void PlayReloadSound()
    {
        var playParams = SoundPlayParams.Following(transform);
        sfxService.PlaySound("weapon_reload", playParams);
    }
}
```

### Environmental Audio

```csharp
public class EnvironmentalAudio : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playParams = new SoundPlayParamsBuilder()
                .AtPosition(transform.position)
                .WithVolume(0.6f)
                .WithLoop(true)
                .Build();

            sfxService.PlaySound("ambient_forest", playParams);
        }
    }
}
```

## Performance Considerations

### Pool Management

```csharp
// Configure pool sizes based on your game's needs:
// - Small games: initialPoolSize = 5-10, maxPoolSize = 20-30
// - Medium games: initialPoolSize = 10-15, maxPoolSize = 40-60
// - Large games: initialPoolSize = 15-25, maxPoolSize = 80-100
```

### Memory Usage

```csharp
// Preload frequently used sounds
// Avoid preloading large, rarely used sounds
// Use compressed audio formats for most sounds
// Use uncompressed only for short, critical sounds
```

## Troubleshooting

### Sound Not Playing
- Check that SoundDefinition is added to the Sound Library
- Verify the sound ID matches the SoundDefinition name
- Ensure audio clips are assigned in the SoundDefinition
- Check that the audio source pool isn't exhausted

### 3D Audio Issues
- Verify SpatialBlend is set correctly (0 = 2D, 1 = 3D)
- Check MaxDistance and RolloffMode settings
- Ensure AudioListener is present in the scene
- Verify positioning parameters are correct

### Performance Issues
- Reduce pool sizes if memory is limited
- Use audio compression for large files
- Limit concurrent sounds with priority settings
- Preload only essential sounds

### Volume Issues
- Check global volume setting
- Verify SoundDefinition volume levels
- Check Audio Mixer Group settings
- Ensure volume multipliers are reasonable

## Static Factory Methods

The SoundPlayParams struct provides convenient static factory methods for common use cases:

```csharp
// Simple factory methods
var defaultParams = SoundPlayParams.Default;
var positionParams = SoundPlayParams.At(Vector3.zero);
var followParams = SoundPlayParams.Following(target);
var volumeParams = SoundPlayParams.WithVolume(0.5f);
var pitchParams = SoundPlayParams.WithPitch(1.2f);
```

## Events System

Subscribe to sound events for advanced audio management:

```csharp
private void OnEnable()
{
    SoundEvents.OnSoundStarted += OnSoundStarted;
    SoundEvents.OnSoundCompleted += OnSoundCompleted;
    SoundEvents.OnSoundStopped += OnSoundStopped;
}

private void OnSoundStarted(string soundId, SoundHandle handle)
{
    Debug.Log($"Sound {soundId} started playing");
}

private void OnSoundCompleted(string soundId, SoundHandle handle)
{
    Debug.Log($"Sound {soundId} finished naturally");
}

private void OnSoundStopped(string soundId, SoundHandle handle)
{
    Debug.Log($"Sound {soundId} was stopped manually");
}
```

## Advanced Examples

### Dynamic Audio System

```csharp
public class DynamicAudioManager : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;

    public void PlayFootstepSound(string surfaceType, Vector3 position)
    {
        string soundId = $"footstep_{surfaceType}";
        var playParams = SoundPlayParams.At(position);
        sfxService.PlaySound(soundId, playParams);
    }

    public void PlayWeaponSound(WeaponType weapon, Vector3 position)
    {
        string soundId = $"weapon_{weapon.ToString().ToLower()}_fire";
        var playParams = new SoundPlayParamsBuilder()
            .AtPosition(position)
            .WithVolume(weapon.Volume)
            .WithPitch(weapon.PitchMultiplier)
            .Build();

        sfxService.PlaySound(soundId, playParams);
    }
}
```

### Audio Feedback System

```csharp
public class AudioFeedbackSystem : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;

    public void PlayDamageSound(float damageAmount, Vector3 position)
    {
        // Scale volume and pitch based on damage
        float volumeMultiplier = Mathf.Clamp(damageAmount / 100f, 0.3f, 1f);
        float pitchMultiplier = 1f + (damageAmount / 200f);

        var playParams = new SoundPlayParamsBuilder()
            .AtPosition(position)
            .WithVolume(volumeMultiplier)
            .WithPitch(pitchMultiplier)
            .Build();

        sfxService.PlaySound("damage_impact", playParams);
    }
}
```

### Ambient Sound Manager

```csharp
public class AmbientSoundManager : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;
    private SoundHandle currentAmbientHandle;

    public void ChangeAmbientSound(string newAmbientId, float fadeTime = 2f)
    {
        // Stop current ambient sound with fade
        if (currentAmbientHandle != null && currentAmbientHandle.isPlaying)
        {
            sfxService.StopSound(currentAmbientHandle, fadeTime);
        }

        // Start new ambient sound
        var playParams = new SoundPlayParamsBuilder()
            .WithLoop(true)
            .WithVolume(0.6f)
            .Build();

        currentAmbientHandle = sfxService.PlaySound(newAmbientId, playParams);
    }
}
```

## Integration with MusicService

The SoundService works seamlessly with the MusicService for comprehensive audio management:

```csharp
public class AudioManager : MonoBehaviour
{
    [SerializeField] private SfxService sfxService;
    [SerializeField] private MusicService musicService;

    public void PlayUISound(string soundId)
    {
        // UI sounds don't need 3D positioning
        sfxService.PlaySound(soundId);
    }

    public void PlayGameplaySound(string soundId, Vector3 position)
    {
        // Gameplay sounds use 3D positioning
        var playParams = SoundPlayParams.At(position);
        sfxService.PlaySound(soundId, playParams);
    }

    public void OnDialogueStart()
    {
        // Duck music and reduce SFX volume for dialogue
        musicService.SetDucking(0.3f, 0.5f);
        sfxService.SetGlobalVolume(0.5f);
    }

    public void OnDialogueEnd()
    {
        // Restore normal audio levels
        musicService.SetDucking(1f, 0.5f);
        sfxService.SetGlobalVolume(1f);
    }
}
```

## Summary

The SoundService provides a robust, feature-rich system for managing sound effects in Unity games. Its key strengths include:

- **Type Safety**: Prevents common audio programming errors
- **Performance**: Efficient audio source pooling and memory management
- **Flexibility**: Supports both 2D and 3D audio with advanced positioning
- **Ease of Use**: Simple API with powerful builder pattern for complex scenarios
- **Editor Integration**: Comprehensive testing and debugging tools
- **Extensibility**: Event system and handle-based control for advanced use cases

Whether you're building a simple 2D game or a complex 3D world, the SoundService provides the tools you need for professional-quality audio implementation.
