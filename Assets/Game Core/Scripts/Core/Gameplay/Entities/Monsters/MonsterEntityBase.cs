using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Gameplay.Entities.Monsters
{
    public abstract class MonsterEntityBase : NetcodeBehaviour, ITeleportableEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ClientNetworkTransform _networkTransform;

        // PROPERTIES: ----------------------------------------------------------------------------

#warning ПЕРЕДЕЛАТЬ НА СЕРВЕР
        public EntityLocation EntityLocation { get; private set; } = EntityLocation.Surface;
#warning ПЕРЕДЕЛАТЬ НА СЕРВЕР
        public Floor CurrentFloor { get; private set; }

        protected ClientNetworkTransform NetworkTransform => _networkTransform;

        // FIELDS: --------------------------------------------------------------------------------

        public static event Action<MonsterEntityBase> OnMonsterSpawnedEvent = delegate { };
        public static event Action<MonsterEntityBase> OnMonsterDespawnedEvent = delegate { };

        public event Action OnEntityTeleportedEvent = delegate { };

        protected event Action<ulong> OnTargetPlayerChangedEvent = delegate { };

        protected ulong TargetPlayerID;

        private static readonly List<MonsterEntityBase> AllMonsters = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake()
        {
#warning ПЕРЕДЕЛАТЬ НА СЕРВЕР
            FindEntityLocation();
        }

        protected void Start()
        {
            StartServerOnly();
            StartClientOnly();
        }

        protected virtual void StartServerOnly()
        {
        }

        protected virtual void StartClientOnly()
        {
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroyServerOnly();
            OnDestroyClientOnly();
        }

        protected virtual void OnDestroyServerOnly()
        {
        }

        protected virtual void OnDestroyClientOnly()
        {
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false)
        {
            if (resetVelocity && TryGetComponent(out Rigidbody rigidBody))
                rigidBody.velocity = Vector3.zero;
            
            _networkTransform.Teleport(position, rotation, transform.localScale);

            SendEntityTeleportedEvent();
        }
        
        public void SetTargetPlayerByID(ulong playerID) => SetTargetPlayerServerRPC(playerID);

        /// <summary>
        /// Вызывается после OnNetworkSpawn!
        /// </summary>
        public void SetEntityLocation(EntityLocation entityLocation) =>
            EntityLocation = entityLocation;

        /// <summary>
        /// Вызывается после OnNetworkSpawn!
        /// </summary>
        public void SetFloor(Floor floor) =>
            CurrentFloor = floor;

        public static void ClearAllMonsters() =>
            AllMonsters.Clear();
        
        public static IReadOnlyList<MonsterEntityBase> GetAllMonsters() => AllMonsters;

        public MonoBehaviour GetMonoBehaviour() => this;

        public Transform GetTransform() => transform;

        public abstract MonsterType GetMonsterType();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void SendEntityTeleportedEvent() =>
            OnEntityTeleportedEvent.Invoke();

        protected static bool IsClientIDMatches(ulong targetClientID) =>
            NetworkHorror.ClientID == targetClientID;

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

        private void SetTargetPlayerLocal(ulong playerID)
        {
            TargetPlayerID = playerID;
            OnTargetPlayerChangedEvent.Invoke(playerID);
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SetTargetPlayerServerRPC(ulong playerID) => SetTargetPlayerClientRPC(playerID);
        
        [ClientRpc]
        private void SetTargetPlayerClientRPC(ulong playerID) => SetTargetPlayerLocal(playerID);

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