using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace NetcodePlus
{
    /// <summary>
    /// Network manager that get instantiated in the game scene only, unlike TheNetwork, it doesn't persist between scenes
    /// </summary>

    public class NetworkGame : MonoBehaviour
    {
        private NetworkSpawner _spawner;
        private NetworkChat _chat;
        private float _updateTimer = 0f;
        private float _statusTimer = 0f;
        private float _totalTimer = 0f;
        private bool _keepValid = false;

        private const float _spawnRefreshRate = 0.5f; //In seconds, interval at which SNetworkObjects are spawned/despawned
        private const float _statusRefreshRate = 10f; //Every 10 seconds, send refresh to lobby to keep the game listed
        private const float _waitTimeMin = 30f; //Minimum wait time before it can self-shutdown

        private static NetworkGame _instance;

        private void Awake()
        {
            _instance = this;
            _spawner = new NetworkSpawner();
            _chat = new NetworkChat();
        }

        private void Start()
        {
            TheNetwork.Get().onTick += TickUpdate;
            Messaging.ListenMsg("action", ReceiveAction);
            Messaging.ListenMsg("variable", ReceiveVariable);
            Messaging.ListenMsg("spawn", ReceiveSpawnList);
            Messaging.ListenMsg("despawn", ReceiveDespawnList);
            Messaging.ListenMsg("change_owner", ReceiveChangeList);
            InitLobbyKeep();
            _chat.Init();
        }

        private void OnDestroy()
        {
            TheNetwork.Get().onTick -= TickUpdate;
            Messaging?.UnListenMsg("action");
            Messaging?.UnListenMsg("variable");
            Messaging?.UnListenMsg("spawn");
            Messaging?.UnListenMsg("despawn");
            Messaging?.UnListenMsg("change_owner");
            SNetworkActions.ClearAll();
            SNetworkVariableBase.ClearAll();
            _chat.Clear();
        }

        private void Update()
        {
            UpdateVisibility();
            UpdateStatus();
        }

        private void TickUpdate()
        {
            if (IsServer && IsReady)
            {
                _spawner.TickUpdate();
            }

            if (IsReady)
            {
                SNetworkVariableBase.TickAll();
            }
        }

        private void InitLobbyKeep()
        {
            if (!IsOnline)
                return; //Not online

            WebClient client = WebClient.Get();
            client?.SetDefaultUrl(NetworkData.Get()._lobbyHost, NetworkData.Get()._lobbyPort);

            LobbyGame game = TheNetwork.Get().GetLobbyGame(); //Make sure we are playing a lobby game
            if (client != null && game != null)
            {
                LobbyPlayer player = game.GetPlayer(TheNetwork.Get().UserID);
                if (player != null)
                    client.SetClientID(player.client_id);

                _keepValid = player != null || IsServer;
            }
        }

        //Spawn Despawn objects based on distance
        private void UpdateVisibility()
        {
            if (!IsServer || !IsReady)
                return;

            //Slow update
            _updateTimer += Time.deltaTime;
            if (_updateTimer < _spawnRefreshRate)
                return;

            _updateTimer = 0f;

            //Optimization Loop
            List<SNetworkOptimizer> objs = SNetworkOptimizer.GetAll();
            foreach (SNetworkOptimizer obj in objs)
            {
                float dist = 999f;
                foreach (SNetworkPlayer character in SNetworkPlayer.GetAll())
                {
                    float pdist = (obj.GetPos() - character.GetPos()).magnitude;
                    dist = Mathf.Min(dist, pdist);
                }

                obj.SetActive(dist < obj.active_range);
            }
        }
        
        private void UpdateStatus()
        {
            //Slow update
            _totalTimer += Time.deltaTime;
            _statusTimer += Time.deltaTime;
            if (_statusTimer < _statusRefreshRate)
                return;

            _statusTimer = 0f;

            KeepAliveLobby();
            KeepAliveLobbyList();
            CheckForShutdown();
        }

        //Send a keep alive to the lobby, to keep the current game listed on the lobby (otherwise it will get deleted if inactivity)
        private async void KeepAliveLobby()
        {
            if (!_keepValid || IsServer)
                return; //Client only

            WebClient client = WebClient.Get();
            if (client != null)
            {
                await client.Send("keep");
            }
        }

        //Send a keep alive to the lobby, to keep the current game listed on the lobby (otherwise it will get deleted if inactivity)
        //Servers sends a bit more info (like list of connected players)
        private async void KeepAliveLobbyList()
        {
            if (!_keepValid || !IsServer)
                return; //Server only

            WebClient web = WebClient.Get();
            LobbyGame game = TheNetwork.Get().GetLobbyGame();
            if (web != null && game != null)
            {
                int index = 0;
                string[] list = new string[TheNetwork.Get().CountClients()];
                foreach (KeyValuePair<ulong, ClientData> pair in TheNetwork.Get().GetClientsData())
                {
                    ClientData client = pair.Value;
                    if (client != null && index < list.Length)
                    {
                        list[index] = client.UserID;
                        index++;
                    }
                }

                if (list.Length > 0)
                {
                    KeepMsg msg = new KeepMsg(game.game_id, list);
                    await web.Send("keep_list", msg);
                }
            }
        }

        //Check if no one is connected, if so shutdown
        private void CheckForShutdown()
        {
            LobbyGame game = TheNetwork.Get().GetLobbyGame(); //Make sure we are playing a dedicated lobby game
            if (IsServer && game != null && game.type == ServerType.DedicatedServer && !game.permanent)
            {
                int connected = TheNetwork.Get().CountClients();
                bool can_shutdown = _totalTimer > _waitTimeMin;
                if (can_shutdown && connected == 0)
                {
                    Application.Quit();
                }
            }
        }

        private void ReceiveAction(ulong client_id, FastBufferReader reader)
        {
            if (client_id != TheNetwork.Get().ClientID)
            {
                reader.ReadValueSafe(out ulong object_id);
                reader.ReadValueSafe(out ushort behaviour_id);
                reader.ReadValueSafe(out ushort type);
                reader.ReadValueSafe(out ushort delivery);
                SNetworkActions handler = SNetworkActions.GetHandler(object_id, behaviour_id);
                handler?.ReceiveAction(client_id, type, reader, (NetworkDelivery)delivery);
            }
        }

        private void ReceiveVariable(ulong client_id, FastBufferReader reader)
        {
            if (client_id != TheNetwork.Get().ClientID)
            {
                reader.ReadValueSafe(out ulong object_id);
                reader.ReadValueSafe(out ushort behaviour_id);
                reader.ReadValueSafe(out ushort variable_id);
                reader.ReadValueSafe(out ushort delivery);
                SNetworkVariableBase handler = SNetworkVariableBase.GetVariable(object_id, behaviour_id, variable_id);
                handler?.ReceiveVariable(client_id, reader, (NetworkDelivery)delivery);
            }
        }

        private void ReceiveSpawnList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetSpawnList list);
                foreach (NetSpawnData data in list.data)
                {
                    _spawner.SpawnClient(data);
                }
            }
        }

        private void ReceiveDespawnList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetDespawnList list);
                foreach (NetDespawnData data in list.data)
                {
                    _spawner.DespawnClient(data.network_id, data.destroy);
                }
            }
        }

        private void ReceiveChangeList(ulong client_id, FastBufferReader reader)
        {
            //Is not server and the sender is the server
            if (!IsServer && client_id == TheNetwork.Get().ServerID)
            {
                reader.ReadNetworkSerializable(out NetChangeList list);
                foreach (NetChangeData data in list.data)
                {
                    _spawner.ChangeOwnerClient(data.network_id, data.owner);
                }
            }
        }

        public NetworkSpawner Spawner { get { return _spawner; } }
        public NetworkChat Chat { get { return _chat; } }
        public NetworkMessaging Messaging { get { return TheNetwork.Get().Messaging; } }

        public bool IsOnline { get { return TheNetwork.Get().IsOnline; } }
        public bool IsServer { get { return TheNetwork.Get().IsServer; } }
        public bool IsClient { get { return TheNetwork.Get().IsClient; } }
        public bool IsReady { get { return TheNetwork.Get().IsReady(); } }

        public static NetworkGame Get()
        {
            if (_instance == null)
                _instance = FindObjectOfType<NetworkGame>();
            return _instance;
        }
    }
}
