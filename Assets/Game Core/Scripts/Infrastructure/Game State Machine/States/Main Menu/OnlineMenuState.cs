using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class OnlineMenuState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OnlineMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SaveSelectionMenuView menuInstance = CreateSaveLoadMenu();
        }

        public void Exit()
        {
            
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static SaveSelectionMenuView CreateSaveLoadMenu() =>
            MenuFactory.Create<SaveSelectionMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked()
        {
            
        }
        
        private void OnOfflineClicked()
        {
            
        }
    }
}