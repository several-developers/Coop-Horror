﻿using System;
using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Infrastructure.Configs.Global.MonstersList;
using GameCore.Enums.Gameplay;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Global;
using Zenject;

namespace GameCore.Infrastructure.Providers.Gameplay.MonstersAI
{
    public class MonstersAIConfigsProvider : IMonstersAIConfigsProvider, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersAIConfigsProvider(
            IConfigsProvider configsProvider,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _gameplayConfigsProvider = gameplayConfigsProvider;
            _monstersListConfig = configsProvider.GetConfig<MonstersListConfigMeta>();
            _monsterAIConfigsKeys = new Dictionary<MonsterType, Type>();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameplayConfigsProvider _gameplayConfigsProvider;
        private readonly MonstersListConfigMeta _monstersListConfig;
        private readonly Dictionary<MonsterType, Type> _monsterAIConfigsKeys;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize() => SetupMonstersAIDictionary();

        public TMonsterConfigType GetConfig<TMonsterConfigType>() where TMonsterConfigType : MonsterAIConfigMeta =>
            _gameplayConfigsProvider.GetConfig<TMonsterConfigType>();

        public bool TryGetMonsterAIConfig(MonsterType monsterType, out MonsterAIConfigMeta monsterAIConfig)
        {
            bool isKeyFound = _monsterAIConfigsKeys.TryGetValue(monsterType, out Type configType);

            if (isKeyFound)
                return TryGetMonsterConfig(configType, out monsterAIConfig);

            monsterAIConfig = null;
            Log.PrintError(log: $"Monster Config Key of type monster '<gb>{monsterType}</gb>' <rb>not found</rb>!");

            return false;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupMonstersAIDictionary()
        {
            IEnumerable<MonstersListConfigMeta.MonsterReference> allMonstersReferences =
                _monstersListConfig.GetAllMonstersReferences();

            foreach (MonstersListConfigMeta.MonsterReference monsterReference in allMonstersReferences)
            {
                MonsterType monsterType = monsterReference.MonsterType;
                MonsterAIConfigMeta monsterAIConfig = monsterReference.MonsterAIConfig;
                Type configType = monsterAIConfig.GetType();

                _monsterAIConfigsKeys.TryAdd(monsterType, configType);

                bool containsKey = _monsterAIConfigsKeys.TryAdd(monsterType, configType);

                if (!containsKey)
                    continue;

                string errorLog = $"Dictionary <rb>already contains key</rb> '<gb>{configType}</gb>'!";
                Log.PrintError(errorLog);
            }
        }

        private bool TryGetMonsterConfig(Type configType, out MonsterAIConfigMeta monsterAIConfig)
        {
            monsterAIConfig = _gameplayConfigsProvider.GetConfig<MonsterAIConfigMeta>(configType);
            bool isConfigFound = monsterAIConfig != null;
            return isConfigFound;
        }
    }
}