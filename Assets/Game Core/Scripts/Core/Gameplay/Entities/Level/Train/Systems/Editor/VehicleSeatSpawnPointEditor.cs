using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Train
{
    [CustomEditor(typeof(VehicleSeatSpawnPoint))]
    public class VehicleSeatSpawnPointEditor : Editor
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly Color Color = new(0.667f, 0.1f, 0.667f, 0.2f);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(VehicleSeatSpawnPoint obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawGizmos;

            if (!drawSphere)
                return;
            
            Color handlesColor = Handles.color;
            Handles.color = Color;

            Handles.DrawWireDisc(center: obj.transform.position, normal: Vector3.up, radius: 0.25f, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}