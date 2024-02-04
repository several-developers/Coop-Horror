using CustomEditors;
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

        [SerializeField, Space(height: 5)]
        private DoorwaysSettings _doorwaysSettings;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Vector3 Size => _size;
        public Vector3 Offset => _offset;
        public DungeonRoomType RoomType => _roomType;
        public DoorwaysSettings DoorwaysSettings => _doorwaysSettings;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool TryGetDoorwaySettings(DoorwayDirection direction, out DoorwaySettings doorwaySettings) =>
            _doorwaysSettings.TryGetDoorwaySettings(direction, out doorwaySettings);
        
        public bool TryGetRandomDoorwaySettings(out DoorwaySettings doorwaySettings) =>
            _doorwaysSettings.TryGetRandomDoorwaySettings(out doorwaySettings);
    }
}