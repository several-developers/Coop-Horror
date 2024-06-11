using System;
using System.Collections.Generic;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Utilities;
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

        public Floor Floor { get; private set; }
        public float Depth => _depth;
        public int AvailableItemsAmount => _availableItemsAmount;
        public bool EditMode => _editMode;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<DungeonItemsSpawner> OnRegisterItemsSpawnerEvent = delegate { };

        private float _depth;
        private int _availableItemsAmount;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Process(RandomStream randomStream, Tile tile)
        {
            if (_disableSpawner)
                return;

            FindDungeonRoot();
            
            _depth = tile.Placement.NormalizedDepth;
            _availableItemsAmount = Random.Range(_itemsAmount.x, _itemsAmount.y + 1);
            
            OnRegisterItemsSpawnerEvent.Invoke(this);
        }

        public void SetSpawnPointPosition(int spawnPointIndex, Vector3 position) =>
            _spawnPoints[spawnPointIndex].SetPosition(position);

        public void DecreaseAvailableItemsAmount() =>
            _availableItemsAmount -= 1;

        public void ClearAvailableItemsAmount() =>
            _availableItemsAmount = 0;
        
        public IReadOnlyList<SpawnPoint> GetAllSpawnPoints() => _spawnPoints;

        public Vector3 GetRandomSpawnWorldPosition()
        {
            Vector3 position = transform.position;

            if (_spawnPoints.Count == 0)
                return position;

            int randomIndex = Random.Range(0, _spawnPoints.Count);
            SpawnPoint spawnPoint = _spawnPoints[randomIndex];
            
            Vector3 randomPositionInSphere = Random.insideUnitSphere;
            randomPositionInSphere *= spawnPoint.Radius;
            randomPositionInSphere.y = 0f;

            Vector3 spawnPosition = spawnPoint.Position + randomPositionInSphere + position;
            return spawnPosition;
        }
        
        public Vector3 GetSpawnPointPosition(int spawnPointIndex) =>
            _spawnPoints[spawnPointIndex].Position;

        public int GetSpawnPointsAmount() =>
            _spawnPoints.Count;

        public bool CanSpawnItem() =>
            _availableItemsAmount > 0 && _spawnPoints.Count > 0;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FindDungeonRoot()
        {
            DungeonRoot dungeonRoot = null;
            Transform parent = transform.parent;
            bool isParentFound = parent != null;
            bool isDungeonRootFound = false;
            int iterations = 0;

            while (isParentFound)
            {
                isDungeonRootFound = parent.TryGetComponent(out dungeonRoot);

                if (isDungeonRootFound)
                    break;
                
                parent = parent.parent;
                isParentFound = parent != null;

                if (iterations > 100)
                {
                    Debug.LogError("Infinity loop!");
                    break;
                }
                
                iterations++;
            }

            if (!isDungeonRootFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }

            Floor = dungeonRoot.Floor;
        }
        
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
    }
}