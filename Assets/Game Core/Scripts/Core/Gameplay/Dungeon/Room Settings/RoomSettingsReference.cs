using System;
using GameCore.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    [Serializable]
    public class RoomSettingsReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private DungeonRoomType _roomType;

        [SerializeField, Required]
        private RoomSettingsMeta _roomSettings;

        // PROPERTIES: ----------------------------------------------------------------------------

        public DungeonRoomType RoomType => _roomType;
        public RoomSettingsMeta RoomSettings => _roomSettings;
    }
}