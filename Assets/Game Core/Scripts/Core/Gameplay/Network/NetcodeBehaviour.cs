using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public abstract class NetcodeBehaviour : NetworkBehaviour
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public bool IsServerOnly => IsServer && IsOwner;
        
        protected bool IsInitialized => _isInitialized;

        // FIELDS: --------------------------------------------------------------------------------

        private bool _isInitialized;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public new bool IsLocalPlayer() =>
            NetworkHorror.ClientID == OwnerClientId;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Update()
        {
            if (!_isInitialized)
                return;

            TickAll();
            
            if (IsServerOnly)
                TickServerOnly();

            if (IsServer)
                TickServer();

            if (IsOwner)
                TickOwner();
            else
                TickNotOwner();
        }

        protected void LateUpdate()
        {
            if (!_isInitialized)
                return;

            LateTickAll();
            
            if (IsServerOnly)
                LateTickServerOnly();

            if (IsServer)
                LateTickServer();

            if (IsOwner)
                LateTickOwner();
            else
                LateTickNotOwner();

            if (IsLocalPlayer())
                LateTickLocalPlayer();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        /// <summary>
        /// Вызывается у сервера и клиента (на каждом объекте).
        /// </summary>
        protected virtual void InitAll()
        {
        }

        /// <summary>
        /// Вызывается только у сервера у локального владельца объекта (у локального игрока-сервера).
        /// </summary>
        protected virtual void InitServerOnly()
        {
        }
        
        /// <summary>
        /// Вызывается только у сервера (на каждом объекте).
        /// </summary>
        protected virtual void InitServer()
        {
        }

        /// <summary>
        /// Вызывается только у локального владельца объекта (у локального игрока).
        /// </summary>
        protected virtual void InitOwner()
        {
        }

        /// <summary>
        /// Вызывается у всех кроме локального владельца объекта (кроме локального игрока).
        /// </summary>
        protected virtual void InitNotOwner()
        {
        }

        /// <summary>
        /// Вызывается у сервера и клиента (на каждом объекте).
        /// </summary>
        protected virtual void TickAll()
        {
        }

        /// <summary>
        /// Вызывается только у сервера у локального владельца объекта (у локального игрока-сервера).
        /// </summary>
        protected virtual void TickServerOnly()
        {
        }
        
        /// <summary>
        /// Вызывается только у сервера (на каждом объекте).
        /// </summary>
        protected virtual void TickServer()
        {
        }

        /// <summary>
        /// Вызывается только у локального владельца объекта (у локального игрока).
        /// </summary>
        protected virtual void TickOwner()
        {
        }

        /// <summary>
        /// Вызывается у всех кроме локального владельца объекта (кроме локального игрока).
        /// </summary>
        protected virtual void TickNotOwner()
        {
        }

        /// <summary>
        /// Вызывается у сервера и клиента (на каждом объекте).
        /// </summary>
        protected virtual void LateTickAll()
        {
        }

        /// <summary>
        /// Вызывается только у сервера у локального владельца объекта (у локального игрока-сервера).
        /// </summary>
        protected virtual void LateTickServerOnly()
        {
        }
        
        /// <summary>
        /// Вызывается только у сервера (на каждом объекте).
        /// </summary>
        protected virtual void LateTickServer()
        {
        }

        /// <summary>
        /// Вызывается только у локального владельца объекта (у локального игрока).
        /// </summary>
        protected virtual void LateTickOwner()
        {
        }

        /// <summary>
        /// Вызывается у всех кроме локального владельца объекта (кроме локального игрока).
        /// </summary>
        protected virtual void LateTickNotOwner()
        {
        }

        protected virtual void LateTickLocalPlayer()
        {
        }

        /// <summary>
        /// Вызывается у сервера и клиента (на каждом объекте).
        /// </summary>
        protected virtual void DespawnAll()
        {
        }

        /// <summary>
        /// Вызывается только у сервера у локального владельца объекта (у локального игрока-сервера).
        /// </summary>
        protected virtual void DespawnServerOnly()
        {
        }
        
        /// <summary>
        /// Вызывается только у сервера (на каждом объекте).
        /// </summary>
        protected virtual void DespawnServer()
        {
        }

        /// <summary>
        /// Вызывается только у локального владельца объекта (у локального игрока).
        /// </summary>
        protected virtual void DespawnOwner()
        {
        }

        /// <summary>
        /// Вызывается у всех кроме локального владельца объекта (кроме локального игрока).
        /// </summary>
        protected virtual void DespawnNotOwner()
        {
        }

        protected bool IsOwnerServer() => IsServerOnly;
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            InitAll();

            if (IsServerOnly)
                InitServerOnly();
            
            if (IsServer)
                InitServer();

            if (IsOwner)
                InitOwner();
            else
                InitNotOwner();

            _isInitialized = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnAll();
            
            if (IsServerOnly)
                DespawnServerOnly();
            
            if (IsServer)
                DespawnServer();

            if (IsOwner)
                DespawnOwner();
            else
                DespawnNotOwner();

            _isInitialized = false;
        }
    }
}