using GameCore.Gameplay.Interactable.MobileHeadquarters;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Interactable.Editor
{
    [CustomEditor(typeof(MobileHQSeat))]
    public class MobileHQSeatEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly Color Color = new(0.667f, 0.1f, 0.667f, 0.2f);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(MobileHQSeat obj, GizmoType gizmo)
        {
            Transform teleportPoint = obj.TeleportPoint;
            
            Color handlesColor = Handles.color;
            Handles.color = Color;

            Handles.DrawWireDisc(center: teleportPoint.position, normal: Vector3.up, radius: 0.25f, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}