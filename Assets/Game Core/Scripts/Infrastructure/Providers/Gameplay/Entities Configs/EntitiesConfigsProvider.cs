using GameCore.Configs.Gameplay.Entities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Gameplay.EntitiesConfigs
{
    public sealed class EntitiesConfigsProvider : AssetsProviderBase, IEntitiesConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public EntitiesConfigsProvider()
        {
            _outdoorChestConfig = Load<OutdoorChestConfigMeta>(path: ConfigsPaths.OutdoorChestConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly OutdoorChestConfigMeta _outdoorChestConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public OutdoorChestConfigMeta GetOutdoorChestConfig() => _outdoorChestConfig;
    }
}