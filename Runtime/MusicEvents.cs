using System;

namespace Ludo.AudioFlux
{
    /// <summary>
    /// Events for music playback
    /// </summary>
    public static class MusicEvents
    {
        public static event Action<string, MusicHandle> OnMusicStarted;
        public static event Action<string, MusicHandle> OnMusicCompleted;
        public static event Action<string, MusicHandle> OnMusicStopped;
        public static event Action<string, MusicHandle> OnIntroCompleted;
        public static event Action<string, string, float> OnCrossfadeStarted;
        public static event Action<string, string> OnCrossfadeCompleted;
        public static event Action<int> OnBeat;
        public static event Action<int> OnBar;
        public static event Action<float> OnDuckingChanged;
        
        internal static void InvokeMusicStarted(string musicId, MusicHandle handle) => OnMusicStarted?.Invoke(musicId, handle);
        internal static void InvokeMusicCompleted(string musicId, MusicHandle handle) => OnMusicCompleted?.Invoke(musicId, handle);
        internal static void InvokeMusicStopped(string musicId, MusicHandle handle) => OnMusicStopped?.Invoke(musicId, handle);
        internal static void InvokeIntroCompleted(string musicId, MusicHandle handle) => OnIntroCompleted?.Invoke(musicId, handle);
        internal static void InvokeCrossfadeStarted(string fromId, string toId, float duration) => OnCrossfadeStarted?.Invoke(fromId, toId, duration);
        internal static void InvokeCrossfadeCompleted(string fromId, string toId) => OnCrossfadeCompleted?.Invoke(fromId, toId);
        internal static void InvokeBeat(int beat) => OnBeat?.Invoke(beat);
        internal static void InvokeBar(int bar) => OnBar?.Invoke(bar);
        internal static void InvokeDuckingChanged(float level) => OnDuckingChanged?.Invoke(level);
    }
}