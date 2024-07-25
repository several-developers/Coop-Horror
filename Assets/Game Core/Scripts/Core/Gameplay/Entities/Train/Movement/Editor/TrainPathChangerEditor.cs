using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    [CustomEditor(typeof(TrainPathChanger))]
    public class TrainPathChangerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(TrainPathChanger obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawSphere;

            if (!drawSphere)
                return;

            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.ZoneColor;
            float radius = obj.GetRadius();

            Handles.DrawWireDisc(center: obj.transform.position, normal: Vector3.up, radius, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}