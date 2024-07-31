using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.RoundManagement
{
    public class RoundManager : IRoundManager, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RoundManager()
        {
            if (!NetworkHorror.IsTrueServer)
                return;
            
            _monstersAlive = new Dictionary<MonsterType, int>();

            SetupMonstersAliveDictionary();

            MonsterEntityBase.OnMonsterSpawnedEvent += OnMonsterSpawned;
            MonsterEntityBase.OnMonsterDespawnedEvent += OnMonsterDespawned;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<MonsterType, int> _monstersAlive;

        private int _currentIndoorDangerValue;
        private int _currentOutdoorDangerValue;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose()
        {
            MonsterEntityBase.OnMonsterSpawnedEvent -= OnMonsterSpawned;
            MonsterEntityBase.OnMonsterDespawnedEvent -= OnMonsterDespawned;
        }

        // TO DO
        public float GetLocationDangerValueMultiplier() => 1f;

        public int GetPlayersAmount() =>
            PlayerEntity.GetPlayersAmount();

        public int GetAlivePlayersAmount() =>
            PlayerEntity.GetAlivePlayersAmount();

        public int GetCurrentIndoorDangerValue() => _currentIndoorDangerValue;

        public int GetCurrentOutdoorDangerValue() => _currentOutdoorDangerValue;
        
        public int GetMonstersCount(MonsterType monsterType) => _monstersAlive[monsterType];

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupMonstersAliveDictionary()
        {
            string[] names = Enum.GetNames(typeof(MonsterType));

            foreach (string name in names)
            {
                bool isParseSuccessful = Enum.TryParse(name, out MonsterType monsterType);

                if (!isParseSuccessful)
                    continue;

                _monstersAlive.TryAdd(monsterType, 0);
            }
        }
        
        private void HandleMonsterAmountChange(MonsterEntityBase monsterEntity, bool increase)
        {
            MonsterType monsterType = monsterEntity.GetMonsterType();
            int monstersAmount = _monstersAlive[monsterType];
            
            monstersAmount += increase ? 1 : -1;
            monstersAmount = Mathf.Max(a: monstersAmount, b: 0);

            _monstersAlive[monsterType] = monstersAmount;
        }
        

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnMonsterSpawned(MonsterEntityBase monsterEntity) =>
            HandleMonsterAmountChange(monsterEntity, increase: true);

        private void OnMonsterDespawned(MonsterEntityBase monsterEntity) =>
            HandleMonsterAmountChange(monsterEntity, increase: false);
    }
}