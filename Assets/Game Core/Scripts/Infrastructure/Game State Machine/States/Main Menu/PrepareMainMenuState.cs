using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Other;
using GameCore.UI.MainMenu.PlayModeSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class PrepareMainMenuState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PrepareMainMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private PlayModeSelectionMenuView _playModeSelectionMenu;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            EnterSignInState();
            return;

            CreatePlayModeSelectionMenu();

            _playModeSelectionMenu.OnOnlineClickedEvent += OnOnlineClicked;
            _playModeSelectionMenu.OnOfflineClickedEvent += OnOfflineClicked;
        }

        public void Exit()
        {
            if (_playModeSelectionMenu != null)
            {
                _playModeSelectionMenu.OnOnlineClickedEvent -= OnOnlineClicked;
                _playModeSelectionMenu.OnOfflineClickedEvent -= OnOfflineClicked;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreatePlayModeSelectionMenu() =>
            _playModeSelectionMenu = MenuFactory.Create<PlayModeSelectionMenuView>();

        private void EnterSignInState() =>
            _gameStateMachine.ChangeState<SignInState>();

        private void EnterOfflineMenuState() =>
            _gameStateMachine.ChangeState<OfflineMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked() => EnterSignInState();

        private void OnOfflineClicked() => EnterOfflineMenuState();
    }
}