using GameCore.Gameplay.Factories;
using GameCore.Gameplay.Managers;
using GameCore.Observers.Gameplay.UI;
using GameCore.UI.Gameplay.PauseMenu;
using GameCore.Utilities;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameplayState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplayState(IGameStateMachine gameStateMachine, IUIObserver uiObserver)
        {
            _gameStateMachine = gameStateMachine;
            _uiObserver = uiObserver;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IUIObserver _uiObserver;

        private PauseMenuView _pauseMenuView;
        private QuitConfirmMenuView _quitConfirmMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            LockCursor();
            CreatePauseMenu();
            CreateQuitConfirmMenuView();

            InputSystemManager.OnOpenPauseMenuEvent += OnOpenPauseMenu;
            
            _pauseMenuView.OnContinueClickedEvent += OnContinueClicked;
            _pauseMenuView.OnQuitClickedEvent += OnQuitClicked;
            
            _quitConfirmMenuView.OnConfirmClickedEvent += OnConfirmQuitClicked;
        }

        public void Exit()
        {
            InputSystemManager.OnOpenPauseMenuEvent -= OnOpenPauseMenu;
            InputSystemManager.OnOpenPauseMenuEvent -= OnOpenPauseMenu;
            
            _pauseMenuView.OnContinueClickedEvent -= OnContinueClicked;
            _pauseMenuView.OnQuitClickedEvent -= OnQuitClicked;
            
            _quitConfirmMenuView.OnConfirmClickedEvent -= OnConfirmQuitClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: true);
        
        private static void UnlockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: false);

        private void CreatePauseMenu() =>
            _pauseMenuView = MenuFactory.Create<PauseMenuView>();

        private void CreateQuitConfirmMenuView() =>
            _quitConfirmMenuView = MenuFactory.Create<QuitConfirmMenuView>();

        private void ShowPauseMenu() =>
            _pauseMenuView.Show();
        
        private void HidePauseMenu() =>
            _pauseMenuView.Hide();

        private void ShowQuitConfirmMenu() =>
            _quitConfirmMenuView.Show();

        private void EnterQuitGameplayState() =>
            _gameStateMachine.ChangeState<QuitGameplayState>();

        private void EnterGameOverState() =>
            _gameStateMachine.ChangeState<GameOverState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOpenPauseMenu()
        {
            UnlockCursor();
            ShowPauseMenu();
            InputSystemManager.SwitchToUI();
        }

        private void OnContinueClicked()
        {
            LockCursor();
            HidePauseMenu();
            InputSystemManager.SwitchToPlayer();
        }

        private void OnQuitClicked() => ShowQuitConfirmMenu();

        private void OnConfirmQuitClicked() => EnterQuitGameplayState();
    }
}