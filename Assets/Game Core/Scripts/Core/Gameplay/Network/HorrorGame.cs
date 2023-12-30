using Cysharp.Threading.Tasks;
using GameCore.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class HorrorGame : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const int CheckCycles = 20;
        private const float CheckDelay = 0.25f;

        private static HorrorGame _instance;

        private TheNetworkHorror _networkHorror;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;
            //StartGame();
        }

        private void Start()
        {
            _networkHorror = TheNetworkHorror.Get();
            _networkHorror.OnConnectEvent += OnConnect;
            _networkHorror.OnCheckApprovalEvent += OnApprove;
            _networkHorror.OnFindPlayerSpawnPositionEvent += FindPlayerPos;
        }

        private void OnDestroy()
        {
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static HorrorGame Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async void StartGame()
        {
            for (int i = 0; i < CheckCycles; i++)
            {
                TheNetworkHorror network = TheNetworkHorror.Get();

                if (network == null)
                {
                    int delay = CheckDelay.ConvertToMilliseconds();

                    bool isCanceled = await UniTask
                        .Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy())
                        .SuppressCancellationThrow();

                    if (isCanceled)
                        break;

                    continue;
                }

                bool isNetworkActive = network.IsActive();

                if (isNetworkActive)
                    return;

                network.StartHost();
                break;
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        // Step 1, connect server
        // Use this to initialize the game on the server
        private void OnConnect()
        {
            if (!_networkHorror.IsServer)
                return;
        }

        // Step 2, approve or not connecting clients
        // Optional, if not defined will just return true
        private bool OnApprove(ulong clientID, int cdata)
        {
            return true;
        }

        // Step 3, select player starting position
        // Optional, if not defined, will spawn at the PlayerSpawn with the same player_id (or at Vector3.zero if none)
        private Vector3 FindPlayerPos(int playerID) =>
            transform.GetRandomPosition(); // TEMP, тут берётся transform HorrorGame, а надо спавнер.
    }
}