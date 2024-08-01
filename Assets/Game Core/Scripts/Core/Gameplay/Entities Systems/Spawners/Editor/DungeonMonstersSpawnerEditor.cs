using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.Spawners.Editor
{
    [CustomEditor(typeof(DungeonMonstersSpawner))]
    public class DungeonMonstersSpawnerEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private DungeonMonstersSpawner _dungeonMonstersSpawner;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            _dungeonMonstersSpawner = target as DungeonMonstersSpawner;
        }
        
        private void OnSceneGUI()
        {
            _dungeonMonstersSpawner = target as DungeonMonstersSpawner;
            
            if (_dungeonMonstersSpawner == null)
                return;

            if (_dungeonMonstersSpawner.EditMode)
            {
                ShowHandle();
                
                Tools.current = Tool.None;
            }
            else
                Tools.current = Tool.Move;
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        public static void RenderCustomGizmo(DungeonMonstersSpawner obj, GizmoType gizmo)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;
            
            Transform transform = obj.transform;
            Vector3 spawnPosition = obj.SpawnPosition;
            
            // Переводим локальную позицию в мировую с учётом локального поворота объекта.
            Vector3 pointPosition = transform.TransformPoint(spawnPosition);
            float radius = obj.SpawnRadius;
                
            Handles.DrawWireDisc(center: pointPosition, normal: Vector3.up, radius, thickness: 3);
            
            Handles.color = handlesColor;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ShowHandle()
        {
            Handles.color = Color.blue;

            EditorGUI.BeginChangeCheck();

            Vector3 roomPosition = _dungeonMonstersSpawner.transform.position;
            Vector3 spawnPointPosition = _dungeonMonstersSpawner.SpawnPosition;
            Vector3 pointPosition = spawnPointPosition + roomPosition;
            Vector3 newTargetPosition = Handles.PositionHandle(pointPosition, Quaternion.identity);

            if (!EditorGUI.EndChangeCheck())
                return;
            
            Undo.RecordObject(_dungeonMonstersSpawner, name: "Move Monster Spawn Point");
                
            Vector3 undoPosition = newTargetPosition - roomPosition;
            _dungeonMonstersSpawner.SetSpawnPosition(undoPosition);
        }
    }
}