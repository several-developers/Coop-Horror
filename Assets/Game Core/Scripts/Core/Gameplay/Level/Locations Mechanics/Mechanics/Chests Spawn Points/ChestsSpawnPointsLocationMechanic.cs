using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.LocationsMechanics
{
    public class ChestsSpawnPointsLocationMechanic : LocationMechanic
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [ShowInInspector]
        private bool _editLocationCenter;
        
        [ShowInInspector]
        private bool _editSpawnPointsCenter;

        [ShowInInspector]
        private bool _drawPossiblePoints = true;
        
        [ShowInInspector]
        private bool _useTemporaryHeight;

        [ShowInInspector]
        private float _temporaryHeight;

        [SerializeField, Space(height: 5)]
        private Vector3 _locationCenter;

        [SerializeField]
        private Vector2 _spawnZoneSize;
        
        [SerializeField]
        private Vector2Int _spawnPointsAmount;

        [SerializeField, Min(0.1f)]
        private float _spawnPointRadius = 1f;

        [SerializeField, Space(height: 5)]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false)]
        private List<Vector3> _spawnPoints = new();

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector3 LocationCenter => _locationCenter;
        public Vector2 SpawnZoneSize => _spawnZoneSize;
        public float SpawnPointRadius => _spawnPointRadius;
        public float TemporaryHeight => _temporaryHeight;
        public bool EditLocationCenter => _editLocationCenter;
        public bool EditSpawnPointsCenter => _editSpawnPointsCenter;
        public bool DrawPossiblePoints => _drawPossiblePoints;
        public bool UseTemporaryHeight => _useTemporaryHeight;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetLocationCenter(Vector3 locationCenter) =>
            _locationCenter = locationCenter;

        public void SetSpawnPoint(int index, Vector3 position)
        {
            if (!IsSpawnPointIndexValid(index))
                return;
            
            _spawnPoints[index] = position;
        }

        public void CreatePossibleSpawnPoints(Action<Vector3, float> handleSpawnPoint)
        {
            // Переводим локальную позицию в мировую с учётом локального поворота объекта.
            Vector3 center = transform.TransformPoint(_locationCenter);
            Vector2 spawnZoneSize = _spawnZoneSize;
            float pointY = center.y;

            float zoneWidth = spawnZoneSize.x;
            float zoneHeight = spawnZoneSize.y;

            float zoneHalfWidth = zoneWidth * 0.5f;
            float zoneHalfHeight = zoneHeight * 0.5f;
            
            int horizontalPointsAmount = _spawnPointsAmount.x;
            int verticalPointsAmount = _spawnPointsAmount.y;

            Vector3 topLeftPosition = new(x: center.x - zoneHalfWidth, y: pointY, z: center.z + zoneHalfHeight);
            Vector3 topRightPosition = new(x: center.x + zoneHalfWidth, y: pointY, z: center.z + zoneHalfHeight);
            Vector3 bottomLeftPosition = new(x: center.x - zoneHalfWidth, y: pointY, z: center.z - zoneHalfHeight);
            Vector3 bottomRightPosition = new(x: center.x + zoneHalfWidth, y: pointY, z: center.z - zoneHalfHeight);

            float verticalStep = (Mathf.Abs(bottomLeftPosition.z) + Mathf.Abs(topLeftPosition.z))
                                 / (verticalPointsAmount - 1);

            float horizontalStep = (Mathf.Abs(bottomLeftPosition.x) + Mathf.Abs(bottomRightPosition.x))
                                   / (horizontalPointsAmount - 1);

            Vector3 verticalStartPoint = bottomLeftPosition;
            Vector3 horizontalStartPoint = bottomLeftPosition;

            for (int x = 0; x < horizontalPointsAmount; x++)
            {
                for (int y = 0; y < verticalPointsAmount; y++)
                {
                    Vector3 localPosition = new(x: horizontalStartPoint.x, y: pointY, z: verticalStartPoint.z);

                    handleSpawnPoint?.Invoke(localPosition, _spawnPointRadius);

                    verticalStartPoint.z += verticalStep;
                }

                verticalStartPoint = bottomLeftPosition;
                horizontalStartPoint.x += horizontalStep;
            }
        }
        
        public int GetSpawnPointsAmount() =>
            _spawnPoints.Count;
        
        public bool TryGetSpawnPointByIndex(int index, out Vector3 spawnPoint)
        {
            spawnPoint = Vector3.zero;
            
            if (!IsSpawnPointIndexValid(index))
                return false;

            spawnPoint = _spawnPoints[index];
            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [Button(buttonSize: 30), DisableInPlayMode]
        private void CreateSpawnPoints()
        {
            _spawnPoints.Clear();
            
            CreatePossibleSpawnPoints(HandleSpawnPoint);
            
            // LOCAL METHODS: -----------------------------
            
            void HandleSpawnPoint(Vector3 pointLocalPosition, float _) =>
                _spawnPoints.Add(pointLocalPosition);
        }

        private bool IsSpawnPointIndexValid(int index) =>
            index >= 0 && index < _spawnPoints.Count;
    }
}