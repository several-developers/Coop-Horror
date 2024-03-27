namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class GameLoopState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameLoopState(IHorrorStateMachine horrorStateMachine)
        {
            _horrorStateMachine = horrorStateMachine;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
        }

        public void Exit()
        {
        }
    }
}