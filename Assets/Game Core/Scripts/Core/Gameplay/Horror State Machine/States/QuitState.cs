namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class QuitState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public QuitState(IHorrorStateMachine horrorStateMachine)
        {

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
        }
    }
}