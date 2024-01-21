using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class PrepareGameState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameState(IHorrorStateMachine horrorStateMachine, IRoadLocationManager roadLocationManager,
            IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _roadLocationManager = roadLocationManager;
            _cancellationTokenSource = new CancellationTokenSource();
            _mobileHeadquartersUtilities = new MobileHeadquartersUtilities(mobileHeadquartersEntity,
                _cancellationTokenSource);

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IRoadLocationManager _roadLocationManager;
        private readonly MobileHeadquartersUtilities _mobileHeadquartersUtilities;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public async UniTaskVoid Enter()
        {
            await MoveMobileHQToTheRoad();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask MoveMobileHQToTheRoad()
        {
            CinemachinePath path = _roadLocationManager.GetPath();
            await _mobileHeadquartersUtilities.MoveMobileHQToThePath(path);
        }

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}