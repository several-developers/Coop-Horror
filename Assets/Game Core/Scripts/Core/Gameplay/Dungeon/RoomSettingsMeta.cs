﻿using CustomEditors;
using GameCore.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    public class RoomSettingsMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Vector3 _size = Vector3.one;
        
        [SerializeField]
        private Vector3 _offset;

        [SerializeField]
        private DungeonRoomType _roomType;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Vector3 Size => _size;
        public Vector3 Offset => _offset;
    }
}