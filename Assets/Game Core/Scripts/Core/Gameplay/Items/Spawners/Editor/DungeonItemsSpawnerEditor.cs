using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Items.Spawners.Editor
{
    [CustomEditor(typeof(DungeonItemsSpawner))]
    public class DungeonItemsSpawnerEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private DungeonItemsSpawner _dungeonItemsSpawner;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            _dungeonItemsSpawner = target as DungeonItemsSpawner;
        }
        
        private void OnSceneGUI()
        {
            _dungeonItemsSpawner = target as DungeonItemsSpawner;
            
            if (_dungeonItemsSpawner == null)
                return;

            if (_dungeonItemsSpawner.EditMode)
            {
                int iterations = _dungeonItemsSpawner.GetSpawnPointsAmount();
                
                for (int i = 0; i < iterations; i++)
                    ShowHandle(i);
                
                Tools.current = Tool.None;
            }
            else
                Tools.current = Tool.Move;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        public static void RenderCustomGizmo(DungeonItemsSpawner obj, GizmoType gizmo)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;
            
            IReadOnlyList<DungeonItemsSpawner.SpawnPoint> allSpawnPoints = obj.GetAllSpawnPoints();
            Vector3 roomPosition = obj.transform.position;
            
            foreach (DungeonItemsSpawner.SpawnPoint spawnPoint in allSpawnPoints)
            {
                Vector3 pointPosition = roomPosition + spawnPoint.Position;
                float radius = spawnPoint.Radius;
                
                Handles.DrawWireDisc(center: pointPosition, normal: Vector3.up, radius, thickness: 3);
            }
            
            Handles.color = handlesColor;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ShowHandle(int index)
        {
            Handles.color = Color.blue;

            EditorGUI.BeginChangeCheck();

            Vector3 roomPosition = _dungeonItemsSpawner.transform.position;
            Vector3 spawnPointPosition = _dungeonItemsSpawner.GetSpawnPointPosition(index);
            Vector3 pointPosition = spawnPointPosition + roomPosition;
            Vector3 newTargetPosition = Handles.PositionHandle(pointPosition, Quaternion.identity);

            if (!EditorGUI.EndChangeCheck())
                return;
            
            Undo.RecordObject(_dungeonItemsSpawner, name: "Move Item Spawn Point");
                
            Vector3 undoPosition = newTargetPosition - roomPosition;
            _dungeonItemsSpawner.SetSpawnPointPosition(index, undoPosition);
        }
    }
}