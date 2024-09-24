using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator.Editor
{
    [CustomEditor(typeof(ElevatorTeleportTrigger))]
    public class ElevatorTeleportTriggerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(ElevatorTeleportTrigger obj, GizmoType gizmo)
        {
            bool drawTrigger = obj.DrawTrigger;

            if (!drawTrigger)
                return;
            
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.PlayerTriggerColor;

            Transform transform = obj.transform;
            Vector3 center = transform.position;
            Vector3 size = transform.localScale;

            Handles.DrawWireCube(center, size);

            Handles.color = handlesColor;
        }
    }
}