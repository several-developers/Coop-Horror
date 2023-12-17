using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
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
            CreatePlayModeSelectionMenu();

            _playModeSelectionMenu.OnOnlineClickedEvent += OnOnlineClicked;
            _playModeSelectionMenu.OnOfflineClickedEvent += OnOfflineClicked;
        }

        public void Exit()
        {
            _playModeSelectionMenu.OnOnlineClickedEvent += OnOnlineClicked;
            _playModeSelectionMenu.OnOfflineClickedEvent += OnOfflineClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreatePlayModeSelectionMenu() =>
            _playModeSelectionMenu = MenuFactory.Create<PlayModeSelectionMenuView>();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        private void EnterOfflineMenuState() =>
            _gameStateMachine.ChangeState<OfflineMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOnlineClicked() => EnterOnlineMenuState();

        private void OnOfflineClicked() => EnterOfflineMenuState();
    }
}