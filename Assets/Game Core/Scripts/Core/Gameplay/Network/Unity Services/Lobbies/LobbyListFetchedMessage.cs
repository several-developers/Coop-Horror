using System.Collections.Generic;

namespace GameCore.Gameplay.Network.UnityServices.Lobbies
{
    public struct LobbyListFetchedMessage
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public LobbyListFetchedMessage(IReadOnlyList<LocalLobby> localLobbies) =>
            LocalLobbies = localLobbies;

        // FIELDS: --------------------------------------------------------------------------------

        public readonly IReadOnlyList<LocalLobby> LocalLobbies;
    }
}