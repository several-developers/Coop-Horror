using GameCore.Configs.Gameplay.Enemies;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public class MonstersAIConfigsProvider : AssetsProviderBase, IMonstersAIConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersAIConfigsProvider()
        {
            _beetleAIConfig = Load<BeetleAIConfigMeta>(path: ConfigsPaths.BeetleAIConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BeetleAIConfigMeta _beetleAIConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public BeetleAIConfigMeta GetBeetleAIConfig() => _beetleAIConfig;
    }
}