namespace GameCore.Gameplay.Network
{
    public partial class TheNetworkHorror
    {
        private class ClientLogic
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ClientLogic(TheNetworkHorror networkHorror) =>
                _networkHorror = networkHorror;

            // FIELDS: --------------------------------------------------------------------------------
            
            private readonly TheNetworkHorror _networkHorror;

            // PUBLIC METHODS: ------------------------------------------------------------------------
            
        }
    }
}