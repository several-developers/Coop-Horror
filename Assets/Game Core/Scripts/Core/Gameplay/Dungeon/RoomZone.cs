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
        
        private readonly Color _zoneColor = new(r: 0.177f, g: 0.965f, b: 1);

        // GAME ENGINE METHODS: -------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_roomSettings == null)
                return;

            Matrix4x4 gizmosMatrix = Gizmos.matrix;
            Color gizmosColor = Gizmos.color;

            DrawCube();
            
            Gizmos.matrix = gizmosMatrix;
            Gizmos.color = gizmosColor;
            
            // LOCAL METHODS: -----------------------------

            void DrawCube()
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = _zoneColor;

                Vector3 size = _roomSettings.Size;
                Vector3 offset = _roomSettings.Offset;
                Vector3 center = Vector3.zero;
                
                center.x += offset.x;
                center.y += size.y * 0.5f;
                center.z += offset.z;
            
                Gizmos.DrawWireCube(center, size);
            }
        }
#endif

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(RoomSettingsMeta roomSettings) =>
            _roomSettings = roomSettings;

        public Vector3 GetRandomDoorPosition() =>
            transform.position;
    }
}