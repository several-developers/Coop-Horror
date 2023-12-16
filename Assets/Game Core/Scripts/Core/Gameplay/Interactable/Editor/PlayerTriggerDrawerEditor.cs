using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Interactable.Editor
{
    [CustomEditor(typeof(PlayerTriggerDrawer))]
    public class PlayerTriggerDrawerEditor : UnityEditor.Editor
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly Color Color = new(1f, 0.667f, 0f, 0.2f);
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(PlayerTriggerDrawer obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawSphere();

            if (!drawSphere)
                return;
            
            Color handlesColor = Handles.color;
            Handles.color = Color;
            float radius = obj.GetColliderRadius();

            Handles.DrawWireDisc(center: obj.transform.position, normal: Vector3.up, radius, thickness: 3);
            // Handles.DrawSolidDisc(obj.transform.position, Vector3.up, radius);

            Handles.color = handlesColor;
        }
    }
}