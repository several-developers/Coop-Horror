using GameCore.Gameplay.Factories;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.InputHandlerTEMP;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Observers.Gameplay.UI;
using GameCore.UI.Gameplay.PauseMenu;
using GameCore.Utilities;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameplaySceneState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplaySceneState(IGameStateMachine gameStateMachine, IHorrorStateMachine horrorStateMachine,
            IConfigsProvider configsProvider, IUIObserver uiObserver)
        {
            _gameStateMachine = gameStateMachine;
            _horrorStateMachine = horrorStateMachine;
            _inputReader = configsProvider.GetInputReader();
            _uiObserver = uiObserver;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly InputReader _inputReader;
        private readonly IUIObserver _uiObserver;

        private PauseMenuView _pauseMenuView;
        private QuitConfirmMenuView _quitConfirmMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            LockCursor();
            CreatePauseMenu();
            CreateQuitConfirmMenuView();
            InitHorrorStateMachine();

            _inputReader.OnPauseEvent += OnOpenPauseMenu;
            
            _pauseMenuView.OnContinueClickedEvent += OnContinueClicked;
            _pauseMenuView.OnQuitClickedEvent += OnQuitClicked;
            
            _quitConfirmMenuView.OnConfirmClickedEvent += OnConfirmQuitClicked;
        }

        public void Exit()
        {
            _inputReader.OnPauseEvent -= OnOpenPauseMenu;
            
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

        private void InitHorrorStateMachine() =>
            _horrorStateMachine.ChangeState<PrepareGameState>();

        private void EnterQuitGameplayState() =>
            _gameStateMachine.ChangeState<QuitGameplaySceneState>();

        private void EnterGameOverState() =>
            _gameStateMachine.ChangeState<GameOverState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOpenPauseMenu()
        {
            UnlockCursor();
            ShowPauseMenu();
            _inputReader.EnableUIInput();
        }

        private void OnContinueClicked()
        {
            LockCursor();
            HidePauseMenu();
            _inputReader.EnableGameplayInput();
        }

        private void OnQuitClicked() => ShowQuitConfirmMenu();

        private void OnConfirmQuitClicked() => EnterQuitGameplayState();
    }
}