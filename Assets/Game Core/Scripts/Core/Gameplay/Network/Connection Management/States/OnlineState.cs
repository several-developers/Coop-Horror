using GameCore.Enums.Global;
using GameCore.Gameplay.PubSub;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal abstract class OnlineState : ConnectionState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        protected OnlineState(ConnectionManager connectionManager, IPublisher<ConnectStatus> connectStatusPublisher)
            : base(connectionManager, connectStatusPublisher)
        {
        }

        // FIELDS: --------------------------------------------------------------------------------

        public const string DtlsConnType = "dtls";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            ConnectStatusPublisher.Publish(message: ConnectStatus.UserRequestedDisconnect);
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            ConnectionManager.ChangeState(ConnectionManager.OfflineState);
        }
    }
}