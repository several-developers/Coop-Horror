using System;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Global.MonstersList
{
    [Serializable]
    public class MonsterReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private MonsterType _monsterType;

        [SerializeField]
        private MonsterAIConfigMeta _monsterAIConfig;
        
        [SerializeField]
        private AssetReferenceGameObject _assetReference;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MonsterType MonsterType => _monsterType;
        public MonsterAIConfigMeta MonsterAIConfig => _monsterAIConfig;
        public AssetReferenceGameObject AssetReference => _assetReference;

        private string Label => $"'Monster: {_monsterType}'";
    }
}