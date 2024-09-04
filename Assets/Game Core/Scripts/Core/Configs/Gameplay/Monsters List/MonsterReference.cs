using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameCore.Configs.Gameplay.MonstersList
{
    [Serializable]
    public class MonsterReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private MonsterType _monsterType;

        [SerializeField, Required]
        private MonsterEntityBase _monsterPrefab;

        [SerializeField]
        private AssetReferenceGameObject _assetReference;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MonsterType MonsterType => _monsterType;
        public MonsterEntityBase MonsterPrefab => _monsterPrefab;
        public AssetReferenceGameObject AssetReference => _assetReference;

        private string Label => $"'Monster: {_monsterType}'";
    }
}