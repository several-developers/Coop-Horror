using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.PlayModeSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class PrepareMainMenuState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareMainMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            PlayModeSelectionMenuView playModeSelectionMenu = CreatePlayModeSelectionMenu();

            playModeSelectionMenu.OnOnlineClickedEvent += OnOnlineClicked;
            playModeSelectionMenu.OnOfflineClickedEvent += OnOfflineClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static PlayModeSelectionMenuView CreatePlayModeSelectionMenu() =>
            MenuFactory.Create<PlayModeSelectionMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked()
        {
            
        }
        
        private void OnOfflineClicked()
        {
            
        }
    }
}