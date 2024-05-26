using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [CustomEditor(typeof(MobileHQPathIDChanger))]
    public class MobileHQPathIDChangerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(MobileHQPathIDChanger obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawSphere;

            if (!drawSphere)
                return;

            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.ZoneColor;
            float radius = obj.GetRadius();
            string pathID = $"Path ID Changer: {obj.PathID}";
            
            Vector3 position = obj.transform.position;
            Vector3 labelPosition = position;
            labelPosition.y += 1.25f;
            
            GUIStyle guiStyle = new(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            
            Handles.Label(labelPosition, pathID, guiStyle);
            Handles.DrawWireDisc(center: position, normal: Vector3.up, radius, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}