using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public partial class TheNetworkHorror
    {
        private class ServerLogic
        {
            // CONSTRUCTORS: --------------------------------------------------------------------------

            public ServerLogic(TheNetworkHorror networkHorror) =>
                _networkHorror = networkHorror;

            // FIELDS: --------------------------------------------------------------------------------
            
            private readonly TheNetworkHorror _networkHorror;

            // PUBLIC METHODS: ------------------------------------------------------------------------
            
        }
    }
}