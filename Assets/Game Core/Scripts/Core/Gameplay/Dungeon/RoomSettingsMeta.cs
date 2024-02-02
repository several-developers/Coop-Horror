using GameCore.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    [CreateAssetMenu(fileName = "Room", menuName = EditorConstants.GameMenuName + "/Room Settings")]
    public class RoomSettingsMeta : ScriptableObject
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