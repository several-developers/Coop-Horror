using System;
using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Systems.Spawners
{
    public class DungeonMonstersSpawner : RandomProp
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [ShowInInspector]
        private bool _editMode;
        
        [SerializeField]
        private bool _disableSpawner;

        [SerializeField]
        private Vector3 _spawnPosition;

        [SerializeField, Min(0f)]
        private float _spawnRadius = 1f;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor Floor { get; private set; }
        public float Depth { get; private set; }
        public bool EditMode => _editMode;
        public Vector3 SpawnPosition => _spawnPosition;
        public float SpawnRadius => _spawnRadius;
        
        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<DungeonMonstersSpawner> OnRegisterMonstersSpawnerEvent = delegate { };

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Process(RandomStream randomStream, Tile tile)
        {
            if (_disableSpawner)
                return;

            FindDungeonRoot();
            
            Depth = tile.Placement.NormalizedDepth;
            
            OnRegisterMonstersSpawnerEvent.Invoke(this);
        }
        
        public void SetSpawnPosition(Vector3 spawnPosition) =>
            _spawnPosition = spawnPosition;
        
        public Vector3 GetRandomSpawnWorldPosition()
        {
            Vector3 randomPositionInSphere = Random.insideUnitSphere;
            randomPositionInSphere *= _spawnRadius;
            randomPositionInSphere.y = 0f;

            Vector3 localSpawnPosition = _spawnPosition + randomPositionInSphere;
            Vector3 spawnPosition = transform.TransformPoint(localSpawnPosition);
            return spawnPosition;
        }
        
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
    }
}