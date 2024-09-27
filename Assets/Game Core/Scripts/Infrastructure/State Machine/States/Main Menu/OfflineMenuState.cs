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
        private SaveSelectionMenuView _saveSelectionMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            CreateSaveSelectionMenu();
            CreateOfflineMenu();

            _offlineMenuView.OnBackButtonClickedEvent += OnBackButtonClicked;
            _offlineMenuView.OnStartButtonClickedEvent += OnStartButtonClicked;
        }
        
        public void Exit()
        {
            _offlineMenuView.OnBackButtonClickedEvent -= OnBackButtonClicked;
            _offlineMenuView.OnStartButtonClickedEvent -= OnStartButtonClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateOfflineMenu() =>
            _offlineMenuView = MenuStaticFactory.Create<OfflineMenuView>();

        private void CreateSaveSelectionMenu() =>
            _saveSelectionMenuView = MenuStaticFactory.Create<SaveSelectionMenuView>();

        private void HideSaveSelectionMenuView() =>
            _saveSelectionMenuView.Hide();

        private void EnterPrepareMainMenuState() =>
            _gameStateMachine.ChangeState<PrepareMainMenuState>();
        
        private void EnterLoadGameplaySceneState() =>
            _gameStateMachine.ChangeState<LoadGameplaySceneState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            HideSaveSelectionMenuView();
            EnterPrepareMainMenuState();
        }

        private void OnStartButtonClicked() => EnterLoadGameplaySceneState();
    }
}