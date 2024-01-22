using GameCore.Enums;
using GameCore.Gameplay.Entities.MobileHeadquarters;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine, IMobileHeadquartersEntity mobileHeadquartersEntity)
        {
            _horrorStateMachine = horrorStateMachine;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
        }

        public void Exit()
        {
        }
    }
}