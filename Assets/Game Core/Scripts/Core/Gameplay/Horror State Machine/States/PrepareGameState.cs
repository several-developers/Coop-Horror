namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class PrepareGameState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareGameState(IHorrorStateMachine horrorStateMachine)
        {
            _horrorStateMachine = horrorStateMachine;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}