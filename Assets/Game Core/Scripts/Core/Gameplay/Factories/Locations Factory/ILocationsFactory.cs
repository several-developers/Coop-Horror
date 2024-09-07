using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Utilities;
using GameCore.Utilities;

namespace GameCore.Gameplay.Factories.Locations
{
    public interface ILocationsFactory : IAddressablesFactory<LocationName>
    {
        UniTask CreateLocation<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager;

        void CreateEntityDynamic<TLocation>(LocationName locationName, SpawnParams<TLocation> spawnParams)
            where TLocation : LocationManager;
    }
}