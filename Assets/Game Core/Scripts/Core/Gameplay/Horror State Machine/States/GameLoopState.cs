using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.Utilities;
using GameCore.Observers.Gameplay.Level;
using GameCore.Observers.Gameplay.Rpc;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState, INetcodeInitBehaviour, INetcodeDespawnBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, INetworkHorror networkHorror,
            ILevelObserver levelObserver, IRpcObserver rpcObserver)
        {
            _horrorStateMachine = horrorStateMachine;
            _networkHorror = networkHorror;
            _rpcObserver = rpcObserver;
            _levelObserver = levelObserver;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly INetworkHorror _networkHorror;
        private readonly ILevelObserver _levelObserver;
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

            _levelObserver.OnLocationLeftEvent += OnLocationLeftServerLogic;
        }

        public void InitClient()
        {
            if (IsServer())
                return;

            _rpcObserver.OnLocationLeftEvent += OnLocationLeftClientLogic;
        }

        public void DespawnServerAndClient()
        {
        }

        public void DespawnServer()
        {
            if (!IsServer())
                return;

            _levelObserver.OnLocationLeftEvent -= OnLocationLeftServerLogic;
        }

        public void DespawnClient()
        {
            if (IsServer())
                return;

            _rpcObserver.OnLocationLeftEvent -= OnLocationLeftClientLogic;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool IsServer() =>
            _networkHorror.IsOwner;

        private void EnterLeaveLocationServerState() =>
            _horrorStateMachine.ChangeState<LeaveLocationServerState>();
        
        private void EnterLeaveLocationClientState() =>
            _horrorStateMachine.ChangeState<LeaveLocationClientState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLocationLeftServerLogic() => EnterLeaveLocationServerState();
        
        private void OnLocationLeftClientLogic() => EnterLeaveLocationClientState();
    }
}