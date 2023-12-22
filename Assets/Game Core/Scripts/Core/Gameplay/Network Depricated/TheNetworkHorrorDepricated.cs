using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using NetcodePlus.Demo;
using NetcodePlus;

namespace GameCore.Gameplay.NetworkDepricated
{
    public class TheNetworkHorrorDepricated : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        public DemoData data;

        // FIELDS: --------------------------------------------------------------------------------

        private const int MaxPlayers = 4;

        private static TheNetworkHorrorDepricated _instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;

            GameModeData.Load();
            PlayerChoiceData.Load();
        }

        private void Start()
        {
            TheNetwork network = TheNetwork.Get();
            network.onConnect += OnConnect;
            network.checkApproval += OnApprove;
            network.onSendWorld += OnSendWorld;
            network.onReceiveWorld += OnReceiveWorld;
            network.findPlayerID += FindPlayerID;
            network.findPlayerPrefab += FindPlayerPrefab;
            network.findPlayerSpawnPos += FindPlayerPos;
            network.onClientQuit += OnDisconnect;

            if (network.IsConnected())
                OnConnect(); //Run now if already connected
        }

        private void OnDestroy()
        {
            TheNetwork network = TheNetwork.Get();
            network.onConnect -= OnConnect;
            network.checkApproval -= OnApprove;
            network.onSendWorld -= OnSendWorld;
            network.onReceiveWorld -= OnReceiveWorld;
            network.findPlayerID -= FindPlayerID;
            network.findPlayerPrefab -= FindPlayerPrefab;
            network.findPlayerSpawnPos -= FindPlayerPos;
            network.onClientQuit -= OnDisconnect;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static TheNetworkHorrorDepricated Get()
        {
            if (_instance == null)
                _instance = FindObjectOfType<TheNetworkHorrorDepricated>();

            return _instance;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        //Step 1, connect server
        //Use this to initialize the game on the server
        private void OnConnect()
        {
            TheNetwork network = TheNetwork.Get();

            if (!network.IsServer) return;

            if (network.ServerType == ServerType.DedicatedServer)
            {
                //Start based on scene selected
                LobbyGame game = network.GetLobbyGame();
                GameModeData gameModeData = GameModeData.GetByScene(game.scene);
                bool isGameModeDataFound = gameModeData != null;
                GameMode gameMode = isGameModeDataFound ? gameModeData.Mode : GameMode.Simple;
                int playersMax = isGameModeDataFound ? gameModeData.PlayersMax : MaxPlayers;

                GameData.Create(gameMode, playersMax);
            }
            else
            {
                //Start based on connection data
                ConnectionData connect = network.GetConnectionData();
                DemoConnectData demoConnectData = NetworkTool.NetDeserialize<DemoConnectData>(connect._extra);
                GameModeData gameModeData = GameModeData.Get(demoConnectData.Mode);

                GameData.Create(demoConnectData.Mode, gameModeData.PlayersMax);
            }
        }

        //Step 2, approve or not connecting clients
        //Optional, if not defined will just return true
        private bool OnApprove(ulong clientID, ConnectionData cdata)
        {
            GameData gdata = GameData.Get();

            if (gdata == null)
                return false; //Game not found

            //Find player id by username (for reconnections)
            PlayerData userPlayer = gdata.GetPlayer(cdata._username);

            if (userPlayer != null && TheNetwork.Get().GetClientByPlayerID(userPlayer.player_id) == null)
                return true; //Already in data

            int count = gdata.CountConnected();
            return count < MaxPlayers;
        }

        //Step 3, assign player ID
        //Optional, if not defined the player_id will be the same than client_id,
        //but this means it will not be possible to reconnect to same game with previous data since netcode assign a new client_id each connection
        private int FindPlayerID(ulong clientID)
        {
            TheNetwork network = TheNetwork.Get();
            GameData gdata = GameData.Get();
            ClientData client = network.GetClient(clientID);

            if (client == null || gdata == null)
                return -1; //Client not or game not found

            DemoConnectData cdata = client.GetExtraData<DemoConnectData>();

            //Find player id by username (for reconnections)
            PlayerData userPlayer = gdata.GetPlayer(client.Username);

            if (userPlayer != null && userPlayer.player_id >= 0 && network.GetClientByPlayerID(userPlayer.player_id) == null)
            {
                userPlayer.connected = true;
                return userPlayer.player_id; //Return only if no other user already connected with this username
            }

            //No player found, assign new player ID
            PlayerData player = gdata.AddNewPlayer(client.Username);

            //Max player count reached
            if (player == null)
                return -1;

            player.connected = true;
            player.character = AssignColor(cdata.Character);

            return player.player_id;
        }

        private string AssignColor(string preferred)
        {
            GameData gameData = GameData.Get();
            GameMode mode = gameData.mode;
            PlayerData playerData = gameData.GetPlayerByCharacter(preferred);
            PlayerChoiceData choice = PlayerChoiceData.Get(mode, preferred);

            if (playerData == null && choice != null)
                return choice.id; //Color not taken, use preferred color

            List<string> colors = new();

            foreach (PlayerChoiceData playerChoiceData in PlayerChoiceData.GetAll(mode))
                colors.Add(playerChoiceData.id);

            PlayerData[] playersData = gameData.players;

            foreach (PlayerData player in playersData)
            {
                if (player != null)
                    colors.Remove(player.character);
            }

            if (colors.Count > 0)
                return colors[Random.Range(0, colors.Count)];

            return ""; //No valid colors
        }

        //Step 4-A, server send GameData to client
        private void OnSendWorld(FastBufferWriter writer)
        {
            //Not using writer.WriteNetworkSerializable like everywhere else,
            //because a small change in the data structure of the save (like if loading an old save) would crash everything
            //instead, using NetworkTool.Serialize allow to be more flexible, and uses the same serialization as when saving to disk
            GameData gameData = GameData.Get();

            if (gameData == null)
            {
                writer.WriteValueSafe(0);
                return;
            }

            byte[] bytes = NetworkTool.Serialize(gameData);
            writer.WriteValueSafe(gameData.mode);
            writer.WriteValueSafe(bytes.Length);

            if (bytes.Length > 0)
                writer.WriteBytesSafe(bytes, bytes.Length);
        }

        //Step 4-B, client receives GameData
        private void OnReceiveWorld(FastBufferReader reader)
        {
            //Not using reader.ReadNetworkSerializable like everywhere else,
            //because a small change in the data structure of the save (like if loading an old save) would crash everything
            //instead, using NetworkTool.Deserialize allow to be more flexible, and uses the same serialization as when saving to disk
            reader.ReadValueSafe(out GameMode mode);

            if (mode == GameMode.None)
                return;

            reader.ReadValueSafe(out int count);
            byte[] bytes = new byte[count];
            reader.ReadBytesSafe(ref bytes, count);
            GameData gameData = NetworkTool.Deserialize<GameData>(bytes);

            if (gameData != null)
                GameData.Override(gameData);
        }

        //Step 5, select player character
        //Optional, if this isnt defined, it will use the default_player prefab assigned in Resources/NetworkData
        private GameObject FindPlayerPrefab(int playerID)
        {
            //Different for each game mode
            GameData gameData = GameData.Get();
            PlayerData playerData = gameData.GetPlayer(playerID);
            NetworkData networkData = NetworkData.Get();

            if (playerData == null)
                return networkData._playerDefault;

            //Specific color
            // PlayerChoiceData playerChoiceData = PlayerChoiceData.Get(gameData.mode, playerData.character);
            //
            // if (playerChoiceData != null)
            //     return playerChoiceData.prefab;

            return networkData._playerDefault;
        }

        //Step 6, select player starting position
        //Optional, if not defined, will spawn at the PlayerSpawn with the same player_id (or at Vector3.zero if none)
        private Vector3 FindPlayerPos(int playerID)
        {
            PlayerSpawn spawn = PlayerSpawn.Get(playerID);

            if (spawn != null)
                return spawn.GetRandomPosition();

            return Vector3.zero;
        }

        //Set our custom flag connected to false when a player disconnects
        private void OnDisconnect(ulong clientID)
        {
            GameData gameData = GameData.Get();
            ClientData clientData = TheNetwork.Get().GetClient(clientID);
            bool isPlayerDataValid = clientData != null && clientData.PlayerID >= 0;

            if (!isPlayerDataValid)
                return;

            PlayerData userPlayer = gameData.GetPlayer(clientData.PlayerID);

            if (userPlayer != null)
                userPlayer.connected = false;
        }
    }
}