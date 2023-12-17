using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.PlayModeSelectionMenu;
using GameCore.UI.MainMenu.SaveLoadMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class OfflineMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OfflineMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SaveLoadMenuView menuInstance = CreateSaveLoadMenu();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static SaveLoadMenuView CreateSaveLoadMenu() =>
            MenuFactory.Create<SaveLoadMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked()
        {
            
        }
        
        private void OnOfflineClicked()
        {
            
        }
    }
}