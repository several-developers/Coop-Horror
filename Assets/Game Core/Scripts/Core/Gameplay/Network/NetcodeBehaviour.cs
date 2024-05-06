using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public class NetcodeBehaviour : NetworkBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private bool _isLocalPlayer;
        private bool _isInitialized;
        private bool _isDespawned;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public new bool IsLocalPlayer() => _isLocalPlayer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Update()
        {
            if (!_isInitialized)
                return;
            
            TickServerAndClient();
            TickServer();
            TickClient();
            TickLocalPlayer();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected virtual void InitServerAndClient()
        {
            
        }
        
        protected virtual void InitServer()
        {
            if (!IsOwner)
                return;
        }
        
        protected virtual void InitClient()
        {
            if (IsOwner)
                return;
        }
        
        protected virtual void InitLocalPlayer()
        {
            if (!_isLocalPlayer)
                return;
        }
        
        protected virtual void InitServerAndClientOnce()
        {
            
        }
        
        protected virtual void InitServerOnce()
        {
            if (!IsOwner)
                return;
        }
        
        protected virtual void InitClientOnce()
        {
            if (IsOwner)
                return;
        }
        
        protected virtual void InitLocalPlayerOnce()
        {
            if (!_isLocalPlayer)
                return;
        }
        
        protected virtual void TickServerAndClient()
        {
        }
        
        protected virtual void TickServer()
        {
            if (!IsOwner)
                return;
        }
        
        protected virtual void TickClient()
        {
            if (IsOwner)
                return;
        }

        protected virtual void TickLocalPlayer()
        {
            if (!_isLocalPlayer)
                return;
        }
        
        protected virtual void DespawnServerAndClient()
        {
            
        }
        
        protected virtual void DespawnServer()
        {
            if (!IsOwner)
                return;
        }
        
        protected virtual void DespawnClient()
        {
            if (IsOwner)
                return;
        }
        
        protected virtual void DespawnLocalPlayer()
        {
            if (!_isLocalPlayer)
                return;
        }
        
        protected virtual void DespawnServerAndClientOnce()
        {
            
        }
        
        protected virtual void DespawnServerOnce()
        {
            if (!IsOwner)
                return;
        }
        
        protected virtual void DespawnClientOnce()
        {
            if (IsOwner)
                return;
        }
        
        protected virtual void DespawnLocalPlayerOnce()
        {
            if (!_isLocalPlayer)
                return;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void MultipleInitialization()
        {
            InitServerAndClient();
            InitServer();
            InitClient();
            InitLocalPlayer();
        }
        
        private void SingleInitialization()
        {
            if (_isInitialized)
                return;

            InitServerAndClientOnce();
            InitServerOnce();
            InitClientOnce();
            InitLocalPlayerOnce();
        }
        
        private void MultipleDespawn()
        {
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
            DespawnLocalPlayer();
        }
        
        private void SingleDespawn()
        {
            if (_isDespawned)
                return;
            
            DespawnServerAndClientOnce();
            DespawnServerOnce();
            DespawnClientOnce();
            DespawnLocalPlayerOnce();
        }

        private void CheckIfLocalPlayer() =>
            _isLocalPlayer = NetworkHorror.ClientID == OwnerClientId;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CheckIfLocalPlayer();
            MultipleInitialization();
            SingleInitialization();
            
            _isInitialized = true;
            _isDespawned = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            MultipleDespawn();
            SingleDespawn();

            _isInitialized = false;
            _isDespawned = true;
        }
    }
}