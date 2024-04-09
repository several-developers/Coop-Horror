using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Levels;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationClientState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationClientState(IHorrorStateMachine horrorStateMachine, ILevelProvider levelProvider)
        {
            _horrorStateMachine = horrorStateMachine;
            _levelProvider = levelProvider;
            
            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILevelProvider _levelProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Enter()
        {
            ClearDungeons();
            ClearLevelProvider();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void ClearDungeons()
        {
            DungeonsManager dungeonsManager = DungeonsManager.Get();
            dungeonsManager.ClearDungeons();
        }

        private void ClearLevelProvider() =>
            _levelProvider.Clear();

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}