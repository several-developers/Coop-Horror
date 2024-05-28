using GameCore.Enums.Global;

namespace GameCore.Gameplay.Level.Locations
{
    public interface ILocationsLoader
    {
        void LoadLocationNetwork(SceneName sceneName);
        void UnloadLastLocation();
    }
}