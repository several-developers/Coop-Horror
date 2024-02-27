namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal abstract class OnlineState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        protected OnlineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        // FIELDS: --------------------------------------------------------------------------------

        public const string DtlsConnType = "dtls";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            //m_ConnectStatusPublisher.Publish(ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}