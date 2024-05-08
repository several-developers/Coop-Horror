using Unity.Netcode;

namespace GameCore.Gameplay.Network
{
    public class NetcodeBehaviour : NetworkBehaviour
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected bool IsInitialized => _isInitialized;

        // FIELDS: --------------------------------------------------------------------------------

        private bool _isLocalPlayer;
        private bool _isInitialized;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public new bool IsLocalPlayer() => _isLocalPlayer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Update()
        {
            if (!_isInitialized)
                return;

            TickServerAndClient();

            if (IsServer)
                TickRealServer();
            
            if (IsOwnedByServer)
                TickOwnerByServer();
            
            if (IsOwner)
                TickServer();
            else
                TickClient();

            if (_isLocalPlayer)
                TickLocalPlayer();
        }

        protected void LateUpdate()
        {
            if (!_isInitialized)
                return;

            LateTickServerAndClient();

            if (IsOwner)
                LateTickServer();
            else
                LateTickClient();

            if (_isLocalPlayer)
                LateTickLocalPlayer();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected virtual void InitServerAndClient()
        {
        }

        protected virtual void InitServer()
        {
        }

        protected virtual void InitClient()
        {
        }

        protected virtual void InitLocalPlayer()
        {
        }

        protected virtual void TickServerAndClient()
        {
        }

        protected virtual void TickServer()
        {
        }

        protected virtual void TickClient()
        {
        }

        protected virtual void TickLocalPlayer()
        {
        }

        protected virtual void LateTickServerAndClient()
        {
        }

        protected virtual void LateTickServer()
        {
        }

        protected virtual void LateTickClient()
        {
        }

        protected virtual void LateTickLocalPlayer()
        {
        }

        protected virtual void DespawnServerAndClient()
        {
        }

        protected virtual void DespawnServer()
        {
        }

        protected virtual void DespawnClient()
        {
        }

        protected virtual void DespawnLocalPlayer()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckIfLocalPlayer() =>
            _isLocalPlayer = NetworkHorror.ClientID == OwnerClientId;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected virtual void TickRealServer()
        {
        }

        protected virtual void TickOwnerByServer()
        {
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CheckIfLocalPlayer();
            InitServerAndClient();

            if (IsOwner)
                InitServer();
            else
                InitClient();

            if (_isLocalPlayer)
                InitLocalPlayer();

            _isInitialized = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            DespawnServerAndClient();

            if (IsOwner)
                DespawnServer();
            else
                DespawnClient();

            if (_isLocalPlayer)
                DespawnLocalPlayer();

            _isInitialized = false;
        }
    }
}