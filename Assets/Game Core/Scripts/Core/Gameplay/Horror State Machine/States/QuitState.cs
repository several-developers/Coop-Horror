namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class QuitState : IState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public QuitState(IHorrorStateMachine horrorStateMachine) =>
            horrorStateMachine.AddState(this);
    }
}