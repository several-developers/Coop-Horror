using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Items.Spawners.Editor
{
    [CustomEditor(typeof(DebugItemsSpawner))]
    public class DebugItemsSpawnerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(DebugItemsSpawner obj, GizmoType gizmo)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;
            
            const float radius = 0.5f;
            Vector3 position = obj.transform.position;
            Vector3 labelPosition = position;
            labelPosition.y += 0.25f;
            
            string itemName = obj.GetItemName();

            GUIStyle guiStyle = new(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            
            Handles.Label(labelPosition, itemName, guiStyle);
            Handles.DrawWireDisc(center: position, normal: Vector3.up, radius, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}