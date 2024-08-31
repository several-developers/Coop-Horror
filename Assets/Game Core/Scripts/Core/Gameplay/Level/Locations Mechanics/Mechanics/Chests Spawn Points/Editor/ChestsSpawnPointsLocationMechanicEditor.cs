using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameCore.Gameplay.Level.LocationsMechanics
{
    [CustomEditor(typeof(ChestsSpawnPointsLocationMechanic))]
    public class ChestsSpawnPointsLocationMechanicEditor : OdinEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private ChestsSpawnPointsLocationMechanic _chestsSpawnPointsLocationMechanic;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            _chestsSpawnPointsLocationMechanic = target as ChestsSpawnPointsLocationMechanic;
        }

        private void OnSceneGUI()
        {
            _chestsSpawnPointsLocationMechanic = target as ChestsSpawnPointsLocationMechanic;

            if (_chestsSpawnPointsLocationMechanic == null)
                return;

            bool editLocationCenter = _chestsSpawnPointsLocationMechanic.EditLocationCenter;
            bool editSpawnPointsCenter = _chestsSpawnPointsLocationMechanic.EditSpawnPointsCenter;
            bool isEditModeOn = editLocationCenter || editSpawnPointsCenter;

            if (!isEditModeOn)
            {
                Tools.current = Tool.Move;
                return;
            }

            Tools.current = Tool.None;

            if (editLocationCenter)
                ShowLocationCenterHandle();

            if (!editSpawnPointsCenter)
                return;

            int spawnPointsAmount = _chestsSpawnPointsLocationMechanic.GetSpawnPointsAmount();

            for (int i = 0; i < spawnPointsAmount; i++)
                ShowSpawnPointHandle(i);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [DrawGizmo(GizmoType.Selected | GizmoType.InSelectionHierarchy)]
        public static void RenderCustomGizmo(ChestsSpawnPointsLocationMechanic obj, GizmoType gizmo)
        {
            DrawSpawnZone(obj);
            DrawPossibleSpawnPoints(obj);
            DrawSpawnPoints(obj);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ShowLocationCenterHandle()
        {
            Handles.color = Color.blue;

            EditorGUI.BeginChangeCheck();

            Vector3 transformPosition = _chestsSpawnPointsLocationMechanic.transform.position;
            Vector3 spawnPointPosition = _chestsSpawnPointsLocationMechanic.LocationCenter;
            Vector3 pointPosition = spawnPointPosition + transformPosition;
            Vector3 newTargetPosition = Handles.PositionHandle(pointPosition, Quaternion.identity);

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(_chestsSpawnPointsLocationMechanic, name: "Move Location Center");

            Vector3 undoPosition = newTargetPosition - transformPosition;
            _chestsSpawnPointsLocationMechanic.SetLocationCenter(undoPosition);
        }

        private void ShowSpawnPointHandle(int index)
        {
            bool isSpawnPointExists =
                _chestsSpawnPointsLocationMechanic.TryGetSpawnPointByIndex(index, out Vector3 spawnPointPosition);

            if (!isSpawnPointExists)
                return;

            Handles.color = Color.blue;

            EditorGUI.BeginChangeCheck();

            bool useTemporaryHeight = _chestsSpawnPointsLocationMechanic.UseTemporaryHeight;
                
            if (useTemporaryHeight)
                spawnPointPosition.y = _chestsSpawnPointsLocationMechanic.TemporaryHeight;

            Vector3 transformPosition = _chestsSpawnPointsLocationMechanic.transform.position;
            Vector3 pointPosition = spawnPointPosition + transformPosition;
            Vector3 newTargetPosition = Handles.PositionHandle(pointPosition, Quaternion.identity);

            if (useTemporaryHeight)
                newTargetPosition.y = _chestsSpawnPointsLocationMechanic.LocationCenter.y;

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObject(_chestsSpawnPointsLocationMechanic, name: "Move Location Chest Spawn Point");

            Vector3 undoPosition = newTargetPosition - transformPosition;
            _chestsSpawnPointsLocationMechanic.SetSpawnPoint(index, undoPosition);
        }

        private static void DrawSpawnZone(ChestsSpawnPointsLocationMechanic obj)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;

            Vector3 locationCenter = obj.LocationCenter;
            Transform transform = obj.transform;

            // Переводим локальную позицию в мировую с учётом локального поворота объекта.
            Vector3 locationWorldCenter = transform.TransformPoint(locationCenter);
            Vector3 spawnZoneSize = new(x: obj.SpawnZoneSize.x, y: 0f, z: obj.SpawnZoneSize.y);

            Handles.DrawWireCube(locationWorldCenter, spawnZoneSize);

            Handles.color = handlesColor;
        }

        private static void DrawPossibleSpawnPoints(ChestsSpawnPointsLocationMechanic obj)
        {
            bool drawPossiblePoints = obj.DrawPossiblePoints;

            if (!drawPossiblePoints)
                return;
            
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.BaseObjectColor;

            Transform transform = obj.transform;
            obj.CreatePossibleSpawnPoints(handleSpawnPoint: HandleSpawnPoint);

            Handles.color = handlesColor;

            // LOCAL METHODS: -----------------------------

            void HandleSpawnPoint(Vector3 pointLocalPosition, float radius)
            {
                Vector3 worldPosition = transform.TransformPoint(pointLocalPosition);
                Handles.DrawWireDisc(center: worldPosition, normal: Vector3.up, radius, thickness: 3f);
            }
        }

        private static void DrawSpawnPoints(ChestsSpawnPointsLocationMechanic obj)
        {
            Color handlesColor = Handles.color;
            Handles.color = ColorsConstants.ZoneColor;

            Transform transform = obj.transform;
            int spawnPointsAmount = obj.GetSpawnPointsAmount();
            float temporaryHeight = obj.TemporaryHeight;
            float radius = obj.SpawnPointRadius;
            bool useTemporaryHeight = obj.UseTemporaryHeight;

            for (int i = 0; i < spawnPointsAmount; i++)
            {
                obj.TryGetSpawnPointByIndex(i, out Vector3 spawnPoint);

                if (useTemporaryHeight)
                    spawnPoint.y = temporaryHeight;
                
                Vector3 worldPosition = transform.TransformPoint(spawnPoint);
                
                Handles.DrawWireDisc(center: worldPosition, normal: Vector3.up, radius, thickness: 3f);
                Handles.Label(worldPosition, text: $"{i}");
            }

            Handles.color = handlesColor;
        }
    }
}