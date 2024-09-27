using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;

namespace GameCore.Infrastructure.Providers.Gameplay.LocationsMeta
{
    public interface ILocationsMetaProvider
    {
        bool TryGetLocationMeta(LocationName locationName, out LocationMeta locationMeta);
    }
}