using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.Level.Locations
{
    public interface ILocationsLoader
    {
        void LoadSceneNetwork(SceneName sceneName);
        void LoadLocationNetwork(LocationName locationName);
        void UnloadLastScene();
    }
}