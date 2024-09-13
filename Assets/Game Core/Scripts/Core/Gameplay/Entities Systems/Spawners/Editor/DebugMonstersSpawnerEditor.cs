using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Spawners.Editor
{
    [CustomEditor(typeof(DebugMonstersSpawner))]
    public class DebugMonstersSpawnerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(DebugMonstersSpawner obj, GizmoType gizmo)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;
            
            Vector3 position = obj.transform.position;
            Vector3 labelPosition = position;
            labelPosition.y += 0.25f;
            
            string monsterName = obj.GetMonsterName();
            float radius = obj.Radius;

            GUIStyle guiStyle = new(GUI.skin.label);
            guiStyle.alignment = TextAnchor.MiddleCenter;
            
            Handles.Label(labelPosition, monsterName, guiStyle);
            Handles.DrawWireDisc(center: position, normal: Vector3.up, radius, thickness: 3);

            Handles.color = handlesColor;
        }
    }
}