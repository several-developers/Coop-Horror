using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Level.Locations
{
    public interface ILocationsLoader
    {
        void LoadLocation(LocationName locationName);
        void UnloadLastLocation();
    }
}