using GameCore.Gameplay.Network.Other;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Network.Editor
{
    [CustomEditor(typeof(PlayerSpawnPoint))]
    public class PlayerSpawnPointEditor : UnityEditor.Editor
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly Color Color = new(0.667f, 0.1f, 0.667f, 0.2f);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(PlayerSpawnPoint obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawGizmos;

            if (!drawSphere)
                return;
            
            Color handlesColor = Handles.color;
            Handles.color = Color;
            float radius = obj.Radius;

            Handles.DrawWireDisc(center: obj.transform.position, normal: Vector3.up, radius, thickness: 3);
            // Handles.DrawSolidDisc(obj.transform.position, Vector3.up, radius);

            Handles.color = handlesColor;
        }
    }
}