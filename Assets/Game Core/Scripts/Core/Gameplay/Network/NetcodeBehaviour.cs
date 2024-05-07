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
        private bool _isDespawned;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public new bool IsLocalPlayer() => _isLocalPlayer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Update()
        {
            if (!_isInitialized)
                return;

            TickServerAndClient();

            if (IsOwner)
                TickServer();
            else
                TickClient();

            if (_isLocalPlayer)
                TickLocalPlayer();
        }

        protected void LateUpdate()
        {
            if (_isInitialized)
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

        protected virtual void InitServerAndClientOnce()
        {
        }

        protected virtual void InitServerOnce()
        {
        }

        protected virtual void InitClientOnce()
        {
        }

        protected virtual void InitLocalPlayerOnce()
        {
        }

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

        protected virtual void DespawnServerAndClientOnce()
        {
        }

        protected virtual void DespawnServerOnce()
        {
        }

        protected virtual void DespawnClientOnce()
        {
        }

        protected virtual void DespawnLocalPlayerOnce()
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

        private void SingleInitialization()
        {
            if (_isInitialized)
                return;

            InitServerAndClientOnce();

            if (IsOwner)
                InitServerOnce();
            else
                InitClientOnce();

            if (_isLocalPlayer)
                InitLocalPlayerOnce();
        }

        private void MultipleInitialization()
        {
            InitServerAndClient();

            if (IsOwner)
                InitServer();
            else
                InitClient();

            if (_isLocalPlayer)
                InitLocalPlayer();
        }

        private void SingleDespawn()
        {
            if (_isDespawned)
                return;

            DespawnServerAndClientOnce();

            if (IsOwner)
                DespawnServerOnce();
            else
                DespawnClientOnce();

            if (_isLocalPlayer)
                DespawnLocalPlayerOnce();
        }

        private void MultipleDespawn()
        {
            DespawnServerAndClient();

            if (IsOwner)
                DespawnServer();
            else
                DespawnClient();

            if (_isLocalPlayer)
                DespawnLocalPlayer();
        }

        private void CheckIfLocalPlayer() =>
            _isLocalPlayer = NetworkHorror.ClientID == OwnerClientId;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CheckIfLocalPlayer();
            SingleInitialization();
            MultipleInitialization();

            _isInitialized = true;
            _isDespawned = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            SingleDespawn();
            MultipleDespawn();

            _isInitialized = false;
            _isDespawned = true;
        }
    }
}