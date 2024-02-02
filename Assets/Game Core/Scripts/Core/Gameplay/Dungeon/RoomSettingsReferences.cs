using System;
using GameCore.Enums;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    [Serializable]
    public class RoomSettingsReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private RoomSettingsReference[] _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetRoomSettings(DungeonRoomType roomType, out RoomSettingsMeta result)
        {
            foreach (RoomSettingsReference reference in _references)
            {
                bool isMatches = reference.RoomType == roomType;
                
                if (!isMatches)
                    continue;

                result = reference.RoomSettings;
                return true;
            }

            result = null;
            return false;
        }
    }
}