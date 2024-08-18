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
            _blindCreatureAIConfig = Load<BlindCreatureAIConfigMeta>(path: ConfigsPaths.BlindCreatureAIConfig);
            _sirenHeadAIConfig = Load<SirenHeadAIConfigMeta>(path: ConfigsPaths.SirenHeadAIConfig);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<MonsterType, MonsterAIConfigMeta> _monsterAIConfigs;
        private readonly GoodClownAIConfigMeta _goodClownAIConfig;
        private readonly EvilClownAIConfigMeta _evilClownAIConfig;
        private readonly BeetleAIConfigMeta _beetleAIConfig;
        private readonly BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        private readonly SirenHeadAIConfigMeta _sirenHeadAIConfig;

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
        public BlindCreatureAIConfigMeta GetBlindCreatureAIConfig() => _blindCreatureAIConfig;
        public SirenHeadAIConfigMeta GetSirenHeadAIConfig() => _sirenHeadAIConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected sealed override T Load<T>(string path)
        {
            var config = base.Load<T>(path);
            
            if (config is MonsterAIConfigMeta monsterConfig)
                AddMonsterAIConfigToDictionary(monsterConfig);
            else
                Log.PrintError(log: $"Monster Config at path <gb>\"{path}\"</gb> <rb>not found</rb>!");
            
            return config;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void AddMonsterAIConfigToDictionary(MonsterAIConfigMeta monsterAIConfig)
        {
            MonsterType monsterType = monsterAIConfig.GetMonsterType();
            _monsterAIConfigs.Add(monsterType, monsterAIConfig);
        }
    }
}