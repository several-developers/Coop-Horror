using System;
using GameCore.Enums;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    [Serializable]
    public class DoorwaySettings
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private Vector3 _localPosition;

        [SerializeField]
        private DoorwayDirection _direction;

        [SerializeField]
        private DoorwayConnectionType _connectionType;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector3 LocalPosition => _localPosition;
        public DoorwayDirection Direction => _direction;
        public DoorwayConnectionType ConnectionType => _connectionType;
    }
}