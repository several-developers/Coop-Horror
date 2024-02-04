using System;
using System.Collections.Generic;
using GameCore.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Dungeon
{
    [Serializable]
    public class DoorwaysSettings
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private List<DoorwaySettings> _settings;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<DoorwaySettings> GetAllSettings() => _settings;

        public bool TryGetDoorwaySettings(DoorwayDirection direction, out DoorwaySettings doorwaySettings)
        {
            int settingsAmount = _settings.Count;
            doorwaySettings = null;
            
            if (settingsAmount == 0)
                return false;

            foreach (DoorwaySettings settings in _settings)
            {
                bool isMatches = settings.Direction == direction;
                
                if (!isMatches)
                    continue;

                doorwaySettings = settings;
                return true;
            }

            return false;
        }
        
        public bool TryGetRandomDoorwaySettings(out DoorwaySettings doorwaySettings)
        {
            int settingsAmount = _settings.Count;
            doorwaySettings = null;
            
            if (settingsAmount == 0)
                return false;

            int randomIndex = Random.Range(0, settingsAmount);
            doorwaySettings = _settings[randomIndex];
            
            return true;
        }
    }
}