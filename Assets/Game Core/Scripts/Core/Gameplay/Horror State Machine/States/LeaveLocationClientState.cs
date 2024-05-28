using GameCore.Gameplay.Dungeons;
using GameCore.Gameplay.Level;

namespace GameCore.Gameplay.HorrorStateMachineSpace
{
    public class LeaveLocationClientState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public LeaveLocationClientState(IHorrorStateMachine horrorStateMachine, ILevelProvider levelProvider,
            DungeonsManager dungeonsManager)
        {
            _horrorStateMachine = horrorStateMachine;
            _levelProvider = levelProvider;
            _dungeonsManager = dungeonsManager;

            horrorStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly ILevelProvider _levelProvider;
        private readonly DungeonsManager _dungeonsManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            ClearDungeons();
            ClearLevelProvider();
            EnterGameLoopState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ClearDungeons() =>
            _dungeonsManager.ClearDungeons();

        private void ClearLevelProvider() =>
            _levelProvider.Clear();

        private void EnterGameLoopState() =>
            _horrorStateMachine.ChangeState<GameLoopState>();
    }
}