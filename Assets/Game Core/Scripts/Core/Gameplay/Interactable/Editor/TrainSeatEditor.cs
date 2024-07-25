using GameCore.Gameplay.Interactable.Train;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Interactable.Editor
{
    [CustomEditor(typeof(TrainSeat))]
    public class TrainSeatEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly Color Color = new(0.667f, 0.1f, 0.667f, 0.2f);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(TrainSeat obj, GizmoType gizmo)
        {
            Transform teleportPoint = obj.TeleportPoint;
            
            Color handlesColor = Handles.color;
            Handles.color = Color;

            Handles.DrawWireDisc(center: teleportPoint.position, normal: Vector3.up, radius: 0.25f, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}