using GameCore.Enums.Global;

namespace GameCore.Gameplay.Levels.Locations
{
    public interface ILocationsLoader
    {
        void LoadLocationNetwork(SceneName sceneName);
        void UnloadLastLocation();
    }
}