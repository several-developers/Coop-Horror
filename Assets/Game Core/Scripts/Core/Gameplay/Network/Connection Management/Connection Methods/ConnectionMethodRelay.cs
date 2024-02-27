using System;
using Cysharp.Threading.Tasks;

namespace GameCore.Gameplay.Network.ConnectionManagement
{
    internal class ConnectionMethodRelay : ConnectionMethodBase
    {
        public ConnectionMethodRelay(ConnectionManager connectionManager, string playerName) : base(connectionManager, playerName)
        {
        }

        public override UniTask SetupHostConnectionAsync()
        {
            throw new NotImplementedException();
        }

        public override UniTask SetupClientConnectionAsync()
        {
            throw new NotImplementedException();
        }
    }
}