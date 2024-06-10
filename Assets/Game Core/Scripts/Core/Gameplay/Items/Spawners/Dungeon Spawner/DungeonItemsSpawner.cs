using System;
using System.Collections.Generic;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Items.Spawners
{
    public class DungeonItemsSpawner : RandomProp
    {
        [Serializable]
        public class SpawnPoint
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private Vector3 _position;

            [SerializeField, Min(0f)]
            private float _radius = 1f;

            // PROPERTIES: ----------------------------------------------------------------------------

            public Vector3 Position => _position;
            public float Radius => _radius;

            private string Label => $"'Position: {_position}',   'Radius: {_radius}'";

            // PUBLIC METHODS: ------------------------------------------------------------------------
            
            public void SetPosition(Vector3 position) =>
                _position = position;
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [ShowInInspector]
        private bool _editMode;
        
        [SerializeField]
        private bool _disableSpawner;

        [SerializeField, MinMaxSlider(minValue: 0, maxValue: 10, showFields: true)]
        private Vector2Int _itemsAmount = new(x: 0, y: 1);

        [SerializeField, Space(height: 5)]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<SpawnPoint> _spawnPoints;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool EditMode => _editMode;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            DungeonRoot.OnDungeonGenerationCompletedEvent += OnDungeonGenerationCompleted;

        private void OnDestroy() =>
            DungeonRoot.OnDungeonGenerationCompletedEvent -= OnDungeonGenerationCompleted;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Process(RandomStream randomStream, Tile tile)
        {
            if (_disableSpawner)
                return;
            
            float depth = tile.Placement.NormalizedDepth;
            
            //Debug.LogWarning("Depth: " + depth);
        }

        public void SetSpawnPointPosition(int spawnPointIndex, Vector3 position) =>
            _spawnPoints[spawnPointIndex].SetPosition(position);
        
        public IReadOnlyList<SpawnPoint> GetAllSpawnPoints() => _spawnPoints;

        public Vector3 GetSpawnPointPosition(int spawnPointIndex) =>
            _spawnPoints[spawnPointIndex].Position;

        public int GetSpawnPointsAmount() =>
            _spawnPoints.Count;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private Vector3 GetRandomNavMeshPositionInRadius(Vector3 position, float radius = 10f,
            NavMeshHit navMeshHit = default)
        {
            float y = position.y;
            position = Random.insideUnitSphere * radius + position;
            position.y = y;

            bool isNavMeshPositionFound = NavMesh.SamplePosition(position, out navMeshHit, radius, areaMask: -1);

            if (isNavMeshPositionFound)
                return navMeshHit.position;

            const string log = "Unable to gem random NavMesh position in radius!";
            Debug.LogWarning(log);
            
            return position;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnDungeonGenerationCompleted(Floor floor)
        {
        }
    }
}