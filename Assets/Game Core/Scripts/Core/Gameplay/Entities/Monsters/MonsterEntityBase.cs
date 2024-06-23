using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Entities.Monsters
{
    public abstract class MonsterEntityBase : NetcodeBehaviour, ITeleportableEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;
        
        [SerializeField, Required]
        protected NavMeshAgent _agent;

        // PROPERTIES: ----------------------------------------------------------------------------

        public EntityLocation EntityLocation { get; private set; } = EntityLocation.LocationSurface;
        public Floor CurrentFloor { get; private set; }
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnEntityTeleportedEvent = delegate { };

        private static readonly List<MonsterEntityBase> AllMonsters = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
            CheckAgentState();
            FindEntityLocation();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Teleport(Vector3 position, Quaternion rotation)
        {
            _agent.enabled = false;
            _networkTransform.Teleport(position, rotation, transform.localScale);
            _agent.enabled = true;
            
            OnEntityTeleportedEvent.Invoke();
        }

        public void SetEntityLocation(EntityLocation entityLocation) =>
            EntityLocation = entityLocation;

        public void SetFloor(Floor floor) =>
            CurrentFloor = floor;

        public static IReadOnlyList<MonsterEntityBase> GetAllMonsters() => AllMonsters;

        public MonoBehaviour GetMonoBehaviour() => this;
        
        public Transform GetTransform() => transform;

        public NavMeshAgent GetAgent() => _agent;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckAgentState()
        {
            if (!_agent.enabled)
            {
                _agent.enabled = true;
                return;
            }

            Log.PrintError(log: "Nav Mesh Agent enabled! Disable it in prefab!");
        }

        private void FindEntityLocation()
        {
            Transform parent = transform.parent;
            bool inDungeon = false;

            while (parent != null)
            {
                bool isDungeonRootFound = parent.TryGetComponent(out DungeonRoot dungeonRoot);

                parent = parent.parent;

                if (!isDungeonRootFound)
                    continue;

                CurrentFloor = dungeonRoot.Floor;
                inDungeon = true;
                break;
            }

            if (inDungeon)
            {
                SetEntityLocation(EntityLocation.Dungeon);
                return;
            }
            
            int randomFloor = Random.Range(1, 4);
            CurrentFloor = (Floor)randomFloor;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            AllMonsters.Add(item: this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            AllMonsters.Remove(item: this);
        }
    }
}