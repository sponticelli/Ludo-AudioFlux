using System;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Events for sound playback
    /// </summary>
    public static class SoundEvents
    {
        public static event Action<string, SoundHandle> OnSoundStarted;
        public static event Action<string, SoundHandle> OnSoundCompleted;
        public static event Action<string, SoundHandle> OnSoundStopped;
        
        internal static void InvokeSoundStarted(string soundId, SoundHandle handle) => OnSoundStarted?.Invoke(soundId, handle);
        internal static void InvokeSoundCompleted(string soundId, SoundHandle handle) => OnSoundCompleted?.Invoke(soundId, handle);
        internal static void InvokeSoundStopped(string soundId, SoundHandle handle) => OnSoundStopped?.Invoke(soundId, handle);
    }
}