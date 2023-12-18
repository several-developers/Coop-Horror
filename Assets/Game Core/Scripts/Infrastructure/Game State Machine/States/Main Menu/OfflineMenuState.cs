using GameCore.Gameplay;
using GameCore.Gameplay.Factories;
using GameCore.UI.MainMenu.OfflineMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class OfflineMenuState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OfflineMenuState(IGameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;

        private OfflineMenuView _offlineMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateSaveSelectionMenu();
            CreateOfflineMenu();

            _offlineMenuView.OnStartButtonClickedEvent += OnStartButtonClicked;
        }
        
        public void Exit() =>
            _offlineMenuView.OnStartButtonClickedEvent -= OnStartButtonClicked;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateOfflineMenu() =>
            _offlineMenuView = MenuFactory.Create<OfflineMenuView>();

        private void EnterLoadGameplayState() =>
            _gameStateMachine.ChangeState<LoadGameplayState>();

        private static void CreateSaveSelectionMenu() =>
            MenuFactory.Create<SaveSelectionMenuView>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartButtonClicked() => EnterLoadGameplayState();
    }
}