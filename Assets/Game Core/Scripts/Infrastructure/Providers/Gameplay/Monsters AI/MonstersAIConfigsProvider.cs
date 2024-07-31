using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Utilities;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public class MonstersAIConfigsProvider : AssetsProviderBase, IMonstersAIConfigsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersAIConfigsProvider()
        {
            _monsterAIConfigs = new Dictionary<MonsterType, MonsterAIConfigMeta>();
            
            _goodClownAIConfig = Load<GoodClownAIConfigMeta>(path: ConfigsPaths.GoodClownAIConfig);
            _evilClownAIConfig = Load<EvilClownAIConfigMeta>(path: ConfigsPaths.EvilClownAIConfig);
            _beetleAIConfig = Load<BeetleAIConfigMeta>(path: ConfigsPaths.BeetleAIConfig);

            AddMonsterAIConfigToDictionary(_goodClownAIConfig);
            AddMonsterAIConfigToDictionary(_evilClownAIConfig);
            AddMonsterAIConfigToDictionary(_beetleAIConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<MonsterType, MonsterAIConfigMeta> _monsterAIConfigs;
        private readonly GoodClownAIConfigMeta _goodClownAIConfig;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly BeetleAIConfigMeta _beetleAIConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetMonsterAIConfig(MonsterType monsterType, out MonsterAIConfigMeta monsterAIConfig)
        {
            bool isMonsterAIConfigFound = _monsterAIConfigs.TryGetValue(monsterType, out monsterAIConfig);

            if (isMonsterAIConfigFound)
                return true;

            Log.PrintError(log: $"Monster AI Config for <gb>{monsterType}</gb> <rb>not found</rb>!");
            return false;
        }

        public GoodClownAIConfigMeta GetGoodClownAIConfig() => _goodClownAIConfig;
        public EvilClownAIConfigMeta GetEvilClownAIConfig() => _evilClownAIConfig;
        public BeetleAIConfigMeta GetBeetleAIConfig() => _beetleAIConfig;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void AddMonsterAIConfigToDictionary(MonsterAIConfigMeta monsterAIConfig)
        {
            MonsterType monsterType = monsterAIConfig.GetMonsterType();
            _monsterAIConfigs.Add(monsterType, monsterAIConfig);
        }
    }
}