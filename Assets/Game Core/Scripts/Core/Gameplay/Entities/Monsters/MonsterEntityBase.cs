using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Storages.Entities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Entities.Monsters
{
    public abstract class MonsterEntityBase : Entity, ITeleportableEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IEntitiesStorage entitiesStorage) =>
            entitiesStorage.AddEntity(gameObject);

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected ClientNetworkTransform NetworkTransform => _networkTransform;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<MonsterEntityBase> OnMonsterSpawnedEvent = delegate { };
        public static event Action<MonsterEntityBase> OnMonsterDespawnedEvent = delegate { };

        public event Action OnEntityTeleportedEvent = delegate { };

        protected event Action<ulong> OnTargetPlayerChangedEvent = delegate { };

        protected ulong TargetPlayerID;

        private static readonly List<MonsterEntityBase> AllMonsters = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void StartServerOnly() => FindEntityLocation();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false)
        {
            if (resetVelocity && TryGetComponent(out Rigidbody rigidBody))
                rigidBody.velocity = Vector3.zero;
            
            _networkTransform.Teleport(position, rotation, transform.localScale);

            SendEntityTeleportedEvent();
        }
        
        public void SetTargetPlayerByID(ulong playerID) => SetTargetPlayerRPC(playerID);

        public static void ClearAllMonsters() =>
            AllMonsters.Clear();
        
        public static IReadOnlyList<MonsterEntityBase> GetAllMonsters() => AllMonsters;

        public abstract MonsterType GetMonsterType();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void SendEntityTeleportedEvent() =>
            OnEntityTeleportedEvent.Invoke();

        // PRIVATE METHODS: -----------------------------------------------------------------------

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

                inDungeon = true;
                SetFloor(dungeonRoot.Floor);
                break;
            }

            if (inDungeon)
            {
                SetEntityLocation(EntityLocation.Dungeon);
                return;
            }

            int randomFloor = Random.Range(1, 4);
            var floor = (Floor)randomFloor;
            
            SetFloor(floor);
        }

        private void SetTargetPlayerLocal(ulong playerID)
        {
            TargetPlayerID = playerID;
            OnTargetPlayerChangedEvent.Invoke(playerID);
        }

        // RPC: -----------------------------------------------------------------------------------

        [Rpc(target: SendTo.Everyone)]
        private void SetTargetPlayerRPC(ulong playerID) => SetTargetPlayerLocal(playerID);
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            AllMonsters.Add(item: this);
            OnMonsterSpawnedEvent.Invoke(this);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            AllMonsters.Remove(item: this);
            OnMonsterDespawnedEvent.Invoke(this);
        }
    }
}