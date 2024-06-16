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
            _goodClownAIConfig = Load<GoodClownAIConfigMeta>(path: ConfigsPaths.GoodClownAIConfig);
            _evilClownAIConfig = Load<EvilClownAIConfigMeta>(path: ConfigsPaths.EvilClownAIConfig);
            _beetleAIConfig = Load<BeetleAIConfigMeta>(path: ConfigsPaths.BeetleAIConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GoodClownAIConfigMeta _goodClownAIConfig;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly BeetleAIConfigMeta _beetleAIConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GoodClownAIConfigMeta GetGoodClownAIConfig() => _goodClownAIConfig;
        public EvilClownAIConfigMeta GetEvilClownAIConfig() => _evilClownAIConfig;
        public BeetleAIConfigMeta GetBeetleAIConfig() => _beetleAIConfig;
    }
}