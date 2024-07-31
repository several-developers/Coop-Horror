using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Balance
{
    [Serializable]
    public class MonstersDangerLevelConfig
    {
        [Serializable]
        public class DangerLevelConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private MonsterDangerLevel _dangerLevel;

            [SerializeField, Min(0)]
            private int _dangerValue;

            // PROPERTIES: ----------------------------------------------------------------------------

            public MonsterDangerLevel DangerLevel => _dangerLevel;
            public int DangerValue => _dangerValue;

            private string Label => $"'Danger Level: {_dangerLevel}',   'Danger Value: {_dangerValue}'";
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<DangerLevelConfig> _dangerLevelConfigs;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public int GetDangerValue(MonsterDangerLevel dangerLevel)
        {
            foreach (DangerLevelConfig dangerLevelConfig in _dangerLevelConfigs)
            {
                bool isMatches = dangerLevelConfig.DangerLevel == dangerLevel;

                if (!isMatches)
                    continue;

                return dangerLevelConfig.DangerValue;
            }
            
            string log = Log.HandleLog($"Danger Value <rb>not found</rb> for Danger Level <gb>{dangerLevel}</gb>");
            Debug.LogWarning(log);
            
            return 0;
        }
    }
}