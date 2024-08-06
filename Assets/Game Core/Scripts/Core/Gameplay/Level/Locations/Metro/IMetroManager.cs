using GameCore.Gameplay.Entities;

namespace GameCore.Gameplay.Level.Locations
{
    public interface IMetroManager
    {
        void TeleportLocalPlayer(bool targetAtSurface);
        void TeleportEntity(ITeleportableEntity entity, bool targetAtSurface);
    }
}