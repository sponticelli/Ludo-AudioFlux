namespace Ludo.AudioFlux
{
    /// <summary>
    /// Interface for the Music service
    /// </summary>
    public interface IMusicService
    {
        MusicHandle PlayMusic(string musicId, MusicPlayParams playParams = default);
        MusicHandle PlayMusic(MusicDefinition musicDef, MusicPlayParams playParams = default);
        void StopMusic(float fadeOutTime = 0f);
        void StopMusic(MusicHandle handle, float fadeOutTime = 0f);
        void PauseMusic();
        void ResumeMusic();
        void SetMusicVolume(float volume);
        void CrossfadeTo(string musicId, float crossfadeDuration = 1f, MusicPlayParams playParams = default);
        void CrossfadeTo(MusicDefinition musicDef, float crossfadeDuration = 1f, MusicPlayParams playParams = default);
        bool IsPlaying();
        bool IsPlaying(string musicId);
        MusicHandle GetCurrentMusic();
        void RegisterMusic(string id, MusicDefinition musicDefinition);
        void PreloadMusic(string musicId);
        string[] GetAvailableMusicIds();
        void SetDucking(float duckingLevel, float duckingTime = 0.5f);
    }
}