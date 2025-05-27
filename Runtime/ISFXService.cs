namespace Ludo.AudioFlux
{
    /// <summary>
    /// Interface for the SFX service
    /// </summary>
    public interface ISFXService
    {
        SoundHandle PlaySound(string soundId, SoundPlayParams playParams = default);
        SoundHandle PlaySound(SoundDefinition soundDef, SoundPlayParams playParams = default);
        void StopSound(SoundHandle handle, float fadeOutTime = 0f);
        void StopAllSounds(float fadeOutTime = 0f);
        void SetGlobalVolume(float volume);
        void PauseAll();
        void ResumeAll();
        bool IsPlaying(string soundId);
        void RegisterSound(string id, SoundDefinition soundDefinition);
        void PreloadSound(string soundId);
        string[] GetAvailableSoundIds();
    }
}