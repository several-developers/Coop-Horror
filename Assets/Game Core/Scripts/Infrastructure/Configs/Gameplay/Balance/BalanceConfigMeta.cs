﻿using GameCore.Enums.Gameplay;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Balance
{
    public class BalanceConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0), SuffixLabel(Seconds, overlay: true)]
        private float _gameRestartDelay = 10f;

        [SerializeField, Min(0f)]
        private float _noiseLoudnessMultiplier = 18f;
        
        [SerializeField, Min(0f)]
        private float _noiseObstacleMultiplier = 0.5f;

        [SerializeField]
        private LocationName _defaultSelectedLocation = LocationName.Forest;

        [SerializeField, Space(5)]
        private MonstersDangerLevelConfig _monstersDangerLevelConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float GameRestartDelay => _gameRestartDelay;
        public float NoiseLoudnessMultiplier => _noiseLoudnessMultiplier;
        public float NoiseObstacleMultiplier => _noiseObstacleMultiplier;
        public LocationName DefaultSelectedLocation => _defaultSelectedLocation;
        public MonstersDangerLevelConfig MonstersDangerLevelConfig => _monstersDangerLevelConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;

        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}