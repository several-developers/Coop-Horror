using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Rpc;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, INetworkHorrorDecorator networkHorrorDecorator,
            IMobileHeadquartersEntity mobileHeadquartersEntity, IRpcObserver rpcObserver)
        {
            _horrorStateMachine = horrorStateMachine;
            _networkHorrorDecorator = networkHorrorDecorator;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _rpcObserver = rpcObserver;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly INetworkHorrorDecorator _networkHorrorDecorator;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly IRpcObserver _rpcObserver;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            InitServerAndClient();
            InitServer();
            InitClient();
        }

        public void Exit()
        {
            DespawnServerAndClient();
            DespawnServer();
            DespawnClient();
        }
        
        public void InitServerAndClient()
        {
            
        }

        public void InitServer()
        {
            if (!IsServer())
                return;

            _mobileHeadquartersEntity.OnLocationLeftEvent += OnLocationLeftServer;
        }

        public void InitClient()
        {
            if (IsServer())
                return;

            _rpcObserver.OnLocationLeftEvent += OnLocationLeftClient;
        }

        public void DespawnServerAndClient()
        {
        }

        public void DespawnServer()
        {
            if (!IsServer())
                return;

            _mobileHeadquartersEntity.OnLocationLeftEvent -= OnLocationLeftServer;
        }

        public void DespawnClient()
        {
            if (IsServer())
                return;

            _rpcObserver.OnLocationLeftEvent -= OnLocationLeftClient;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsServer() =>
            _networkHorrorDecorator.IsServer();

        private void EnterLeaveLocationServerState() =>
            _horrorStateMachine.ChangeState<LeaveLocationServerState>();
        
        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLocationLeftServer() => EnterLeaveLocationServerState();
        
        private void OnLocationLeftClient() => EnterLeaveLocationClientState();
    }
}