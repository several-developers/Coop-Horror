using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Level
{
    [CustomEditor(typeof(FireExitTeleportPointDrawer))]
    public class FireExitTeleportPointDrawerEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const float Radius = 0.25f;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(FireExitTeleportPointDrawer obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawSphere;

            if (!drawSphere)
                return;

            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.ZoneColor;

            Handles.DrawWireDisc(center: obj.transform.position, normal: Vector3.up, Radius, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}