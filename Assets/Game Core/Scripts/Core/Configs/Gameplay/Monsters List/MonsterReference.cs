using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Beetle;
using Sirenix.OdinInspector;
using UnityEngine;

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

        // PROPERTIES: ----------------------------------------------------------------------------

        public MonsterType MonsterType => _monsterType;
        public MonsterEntityBase MonsterPrefab => _monsterPrefab;

        private string Label => $"'Monster: {_monsterType}'";
    }
}