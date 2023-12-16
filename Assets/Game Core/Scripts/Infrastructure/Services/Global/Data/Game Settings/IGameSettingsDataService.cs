namespace GameCore.Infrastructure.Services.Global.Data
{
    public interface IGameSettingsDataService
    {
        void SetSoundVolume(float volume, bool autoSave = true);
        void SetMusicVolume(float volume, bool autoSave = true);
        float GetSoundVolume();
        float GetMusicVolume();
    }
}