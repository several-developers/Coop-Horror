using System.IO;
using UnityEngine;

namespace NetcodePlus
{
    /// <summary>
    /// Network config data (only one file)
    /// </summary>

    [CreateAssetMenu(fileName = "NetworkData", menuName = "Netcode/NetworkData", order = 0)]
    public class NetworkData : ScriptableObject
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Header("Game Server")]
        public ushort _gamePort = 7700;         //Port used by Netcode for game server (default)
        public int _playersMax = 4;             //Maximum number of players in each game
        public GameObject _playerDefault;       //Default player prefab
        
        [Header("Lobby Server")]
        public string _lobbyHost = "127.0.0.1";  //Default url (or IP) where the lobby is located
        public ushort _lobbyPort = 7800;        //Port used by Netcode for Lobby Server
        public ServerType _lobbyGameType = ServerType.DedicatedServer; //Which type of game server will the lobby create ?  The lobby itself is always dedicated.
        public int _lobbyRoomsMax = 10;                                   //Maximum number of rooms in lobby

        [Header("Server Launcher")]            //For lobby in dedicated server mode only
        public ushort _gamePortMin = 7700;     //If game server is created by lobby, port will be selected in this range (first game server is 7700, second is 7701...)
        public ushort _gamePortMax = 7799;     //If game server is created by lobby, port will be selected in this range (first game server is 7700, second is 7701...)
        public string _gamePathWindows = "../ServerGame/Survival Engine Online.exe"; //Absolute path, unless it starts with ./ or .. , then Relative to Application.dataPath
        public string _gamePathLinux = "/server/game/ServerGame.x86_64"; //Absolute path, unless it starts with ./ or .. , then Relative to Application.dataPath
        public string[] _gameHosts;             //Url of available game servers, if this is empty, it will use the same as lobby_url instead

        [Header("Authentication")]
        public AuthenticatorType _authType = AuthenticatorType.Test; //Change this based on the platform you are building to
        public bool _authAutoLogout;                       //If true, will auto-logout at start, useful to test different users on many windows of same PC 

        // PROPERTIES: ----------------------------------------------------------------------------

        public ushort GamePort => _gamePort;
        public int PlayersMax => _playersMax;
        public GameObject PlayerDefault => _playerDefault;
        
        public string LobbyHost => _lobbyHost;
        public ushort LobbyPort => _lobbyPort;
        public ServerType LobbyGameType => _lobbyGameType;
        public int LobbyRoomsMax => _lobbyRoomsMax;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public string GetExePath()
        {
            string path = GetExePathSetting();
            string fullPath = path;
            
            if (path.StartsWith(".."))
                fullPath = Path.Combine(Application.dataPath, path);
            else if (path.StartsWith("./"))
                fullPath = Path.Combine(Application.dataPath, path.Remove(0, 1));
            
            return fullPath;
        }

        public string GetExePathSetting()
        {
#if UNITY_STANDALONE_WIN
            return _gamePathWindows;
#elif UNITY_STANDALONE_LINUX
            return game_path_linux;
#else
            return "";
#endif
        }

        public static NetworkData Get()
        {
            TheNetwork net = TheNetwork.Get();
            
            if (net != null && net.data)
                return net.data;
            
            return null;
        }
    }
}