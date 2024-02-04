using System.Collections.Generic;
using GameCore.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeon
{
    public class RoomZone : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required, InlineEditor]
        private RoomSettingsMeta _roomSettings;

        // FIELDS: --------------------------------------------------------------------------------

        private const float DoorwayRadius = 0.2f;

        private readonly Color _zoneColor = new(r: 0.18f, g: 0.97f, b: 1);
        private readonly Color _doorwayColor = new(r: 1f, g: 0.66f, b: 0.061f);
        private readonly Color _northColor = new(r: 0f, g: 0.13f, b: 1f);
        private readonly Color _southColor = new(r: 1f, g: 0f, b: 0.7f);
        private readonly Color _westColor = new(r: 0.28f, g: 1f, b: 0.07f);
        private readonly Color _eastColor = new(r: 1f, g: 0.033f, b: 0.033f);

        private int _lastRoomIndex;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_roomSettings == null)
                return;

            Matrix4x4 gizmosMatrix = Gizmos.matrix;
            Color gizmosColor = Gizmos.color;

            Gizmos.matrix = transform.localToWorldMatrix;

            DrawRoomZone();
            DrawDoorways();

            Gizmos.matrix = gizmosMatrix;
            Gizmos.color = gizmosColor;

            // LOCAL METHODS: -----------------------------

            void DrawRoomZone()
            {
                Gizmos.color = _zoneColor;

                Vector3 size = _roomSettings.Size;
                Vector3 offset = _roomSettings.Offset;
                Vector3 center = Vector3.zero;

                center.x += offset.x;
                center.y += size.y * 0.5f;
                center.z += offset.z;

                Gizmos.DrawWireCube(center, size);
            }

            void DrawDoorways()
            {
                IReadOnlyList<DoorwaySettings> doorwaySettings = _roomSettings.DoorwaysSettings.GetAllSettings();

                foreach (DoorwaySettings settings in doorwaySettings)
                {
                    Vector3 position = settings.LocalPosition;

                    Gizmos.color = _doorwayColor;
                    Gizmos.DrawWireSphere(position, DoorwayRadius);

                    DoorwayDirection direction = settings.Direction;
                    Color directionColor = GetDoorwayDirectionColor(direction);
                    Vector3 directionOffset = GetDoorwayDirectionOffset(direction);

                    Gizmos.color = directionColor;
                    Gizmos.DrawLine(position, position + directionOffset);
                }
            }

            Color GetDoorwayDirectionColor(DoorwayDirection direction)
            {
                return direction switch
                {
                    DoorwayDirection.North => _northColor,
                    DoorwayDirection.South => _southColor,
                    DoorwayDirection.West => _westColor,
                    DoorwayDirection.East => _eastColor,
                    _ => Color.black
                };
            }

            Vector3 GetDoorwayDirectionOffset(DoorwayDirection direction)
            {
                return direction switch
                {
                    DoorwayDirection.North => Vector3.forward,
                    DoorwayDirection.South => Vector3.back,
                    DoorwayDirection.West => Vector3.left,
                    DoorwayDirection.East => Vector3.right,
                    _ => Vector3.zero
                };
            }
        }
#endif

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(RoomSettingsMeta roomSettings, int lastRoomIndex)
        {
            _roomSettings = roomSettings;
            _lastRoomIndex = lastRoomIndex;
        }

        public RoomSettingsMeta GetRoomSettings() => _roomSettings;
    }
}