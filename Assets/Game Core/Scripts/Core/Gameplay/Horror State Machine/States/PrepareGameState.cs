using Cinemachine;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Locations;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class PrepareGameState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameState(IHorrorStateMachine horrorStateMachine, IRoadLocationManager roadLocationManager,
            IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _roadLocationManager = roadLocationManager;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IRoadLocationManager _roadLocationManager;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            MoveMobileHQToTheRoad();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MoveMobileHQToTheRoad()
        {
            CinemachinePath path = _roadLocationManager.GetPath();
            _mobileHeadquartersEntity.ChangePath(path);
        }

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}