using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;

namespace GameCore.Infrastructure.Providers.Gameplay.LocationsMeta
{
    public interface ILocationsMetaProvider
    {
        IEnumerable<LocationMeta> GetAllLocationsMeta();
        bool TryGetLocationMeta(LocationName locationName, out LocationMeta locationMeta);
    }
}