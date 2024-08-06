using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations.Editor
{
    [CustomEditor(typeof(MetroLocationManager))]
    public class RoadLocationManagerEditor : OdinEditor
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.InSelectionHierarchy |
                   GizmoType.NotInSelectionHierarchy)]
        public static void RenderCustomGizmo(MetroLocationManager obj, GizmoType gizmo)
        {
            bool drawDebug = obj.DrawDebug;

            if (!drawDebug)
                return;
            
            IReadOnlyList<RoadPathReference> allEnterPathsReferences = obj.GetAllEnterPathsReferences();

            foreach (RoadPathReference pathReference in allEnterPathsReferences)
            {
                bool isValid = pathReference.Path != null;

                if (!isValid)
                    continue;
                
                Vector3 position = pathReference.Path.EvaluatePosition(pos: 0f);
                Vector3 labelPosition = position;
                labelPosition.y += 1.25f;

                string pathID = $"Path ID: {pathReference.PathID}";
                
                GUIStyle guiStyle = new(GUI.skin.label);
                guiStyle.alignment = TextAnchor.MiddleCenter;
            
                Handles.Label(labelPosition, pathID, guiStyle);
            }
        }
    }
}