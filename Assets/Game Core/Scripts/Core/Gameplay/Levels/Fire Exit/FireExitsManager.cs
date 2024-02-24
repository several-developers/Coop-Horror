using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels
{
    public class FireExitsManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelManager levelManager) =>
            _levelManager = levelManager;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private FireExit[] _stairsFireExits;

        // FIELDS: --------------------------------------------------------------------------------

        private ILevelManager _levelManager;
        private TheNetworkHorror _networkHorror;
        private RpcCaller _rpcCaller;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            _networkHorror = TheNetworkHorror.Get();
            _rpcCaller = RpcCaller.Get();

            foreach (FireExit stairsFireExit in _stairsFireExits)
                _levelManager.AddStairsFireExit(stairsFireExit.Floor, stairsFireExit);

            _rpcCaller.OnTeleportToFireExitEvent += OnTeleportToFireExit;
        }

        private void OnDestroy() =>
            _rpcCaller.OnTeleportToFireExitEvent -= OnTeleportToFireExit;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation)
        {
            bool isClientIDMatches = clientID == _networkHorror.OwnerClientId;

            if (!isClientIDMatches)
                return;

            bool isPlayerFound = _networkHorror.TryGetPlayerEntity(clientID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;

            FireExit fireExit;

            // Reversed
            bool isFireExitFound = isInStairsLocation
                ? _levelManager.TryGetOtherFireExit(floor, out fireExit)
                : _levelManager.TryGetStairsFireExit(floor, out fireExit);

            if (!isFireExitFound)
                return;
            
            Transform teleportPoint = fireExit.GetTeleportPoint();
            Vector3 position = teleportPoint.position;
            Quaternion rotation = teleportPoint.rotation;
            
            playerEntity.TeleportPlayer(position, rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTeleportToFireExit(ulong clientID, Floor floor, bool isInStairsLocation) =>
            TeleportToFireExit(clientID, floor, isInStairsLocation);
    }
}