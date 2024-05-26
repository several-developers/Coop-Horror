using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [CustomEditor(typeof(MobileHQSpeedChangerZone))]
    public class MobileHQSpeedChangerZoneEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(MobileHQSpeedChangerZone obj, GizmoType gizmo)
        {
            bool drawSphere = obj.DrawSphere;

            if (!drawSphere)
                return;

            Color gizmosColor = Gizmos.color;
            Gizmos.color = ColorsConstants.ZoneColor;
            float radius = obj.GetRadius();
            float speedPercent = obj.SpeedPercent;
            float finalSpeedPercent = speedPercent * 100f;
            string label = $"Speed Zone: {finalSpeedPercent:F0}%";
             
            Vector3 position = obj.transform.position;
            Vector3 labelPosition = position;
            labelPosition.y += radius * 0.5f;
            
            GUIStyle guiStyle = new(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            
            Handles.Label(labelPosition, label, guiStyle);
            Gizmos.DrawWireSphere(center: position, radius);

            Gizmos.color = gizmosColor;
        }
    }
}