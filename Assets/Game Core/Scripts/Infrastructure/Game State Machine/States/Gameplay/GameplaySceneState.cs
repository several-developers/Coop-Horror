using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Factories;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.InputManagement;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Gameplay.GameOverMenu;
using GameCore.UI.Gameplay.GameOverWarningMenu;
using GameCore.UI.Gameplay.LocationsSelectionMenu;
using GameCore.UI.Gameplay.PauseMenu;
using GameCore.UI.Gameplay.Quests;
using GameCore.UI.Gameplay.Quests.ActiveQuests;
using GameCore.UI.Gameplay.RewardMenu;
using GameCore.Utilities;
using Zenject;

namespace GameCore.Infrastructure.StateMachine
{
    public class GameplaySceneState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplaySceneState(IGameStateMachine gameStateMachine, IHorrorStateMachine horrorStateMachine,
            IMobileHeadquartersEntity mobileHeadquartersEntity, IConfigsProvider configsProvider,
            IGameManagerDecorator gameManagerDecorator, DiContainer diContainer)
        {
            _gameStateMachine = gameStateMachine;
            _horrorStateMachine = horrorStateMachine;
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _gameManagerDecorator = gameManagerDecorator;
            _diContainer = diContainer;
            _inputReader = configsProvider.GetInputReader();

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly DiContainer _diContainer;
        private readonly InputReader _inputReader;

        private PauseMenuView _pauseMenuView;
        private QuitConfirmMenuView _quitConfirmMenuView;
        private QuestsSelectionMenuView _questsSelectionMenuView;
        private LocationsSelectionMenuView _locationsSelectionMenuView;
        private GameOverMenuView _gameOverMenuView;
        private GameOverWarningMenuView _gameOverWarningMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            LockCursor();
            EnableGameplayInput();

            CreateActiveQuestsView(); // TEMP
            CreateQuestsSelectionMenuView(); // TEMP
            CreateLocationsSelectionMenuView(); // TEMP
            CreateGameOverMenu(); // TEMP
            CreateRewardMenu(); // TEMP
            CreateGameOverWarningMenuView(); // TEMP
            CreatePauseMenu(); // TEMP
            CreateQuitConfirmMenuView(); // TEMP

            InitHorrorStateMachine();

            _inputReader.OnPauseEvent += OnOpenPauseMenu;

            _mobileHeadquartersEntity.OnOpenQuestsSelectionMenuEvent += OnOpenQuestsSelectionMenu;
            _mobileHeadquartersEntity.OnOpenLocationsSelectionMenuEvent += OnOpenLocationsSelectionMenu;
            _mobileHeadquartersEntity.OnOpenGameOverWarningMenuEvent += OnOpenGameOverWarningMenu;
            
            _pauseMenuView.OnContinueClickedEvent += OnContinueClicked;
            _pauseMenuView.OnQuitClickedEvent += OnQuitClicked;

            _gameOverWarningMenuView.OnConfirmClickedEvent += OnGameOverWarningConfirmClicked;
            _gameOverWarningMenuView.OnCancelClickedEvent += OnGameOverWarningCancelClicked;

            _quitConfirmMenuView.OnConfirmClickedEvent += OnQuitConfirmClicked;

            _gameManagerDecorator.OnGameStateChangedEvent += GameStateChanged;
        }

        public void Exit()
        {
            _inputReader.OnPauseEvent -= OnOpenPauseMenu;

            _mobileHeadquartersEntity.OnOpenQuestsSelectionMenuEvent -= OnOpenQuestsSelectionMenu;
            _mobileHeadquartersEntity.OnOpenLocationsSelectionMenuEvent -= OnOpenLocationsSelectionMenu;
            _mobileHeadquartersEntity.OnOpenGameOverWarningMenuEvent -= OnOpenGameOverWarningMenu;

            _pauseMenuView.OnContinueClickedEvent -= OnContinueClicked;
            _pauseMenuView.OnQuitClickedEvent -= OnQuitClicked;

            _gameOverWarningMenuView.OnConfirmClickedEvent -= OnGameOverWarningConfirmClicked;
            _gameOverWarningMenuView.OnCancelClickedEvent -= OnGameOverWarningCancelClicked;

            _quitConfirmMenuView.OnConfirmClickedEvent -= OnQuitConfirmClicked;
            
            _gameManagerDecorator.OnGameStateChangedEvent -= GameStateChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: true);

        private static void UnlockCursor() =>
            GameUtilities.ChangeCursorLockState(isLocked: false);

        private void EnableGameplayInput() =>
            _inputReader.EnableGameplayInput();

        private void CreateActiveQuestsView() =>
            MenuFactory.Create<ActiveQuestsView>(_diContainer);

        private void CreateQuestsSelectionMenuView() =>
            _questsSelectionMenuView = MenuFactory.Create<QuestsSelectionMenuView>(_diContainer);

        private void CreateLocationsSelectionMenuView() =>
            _locationsSelectionMenuView = MenuFactory.Create<LocationsSelectionMenuView>(_diContainer);

        private void CreateGameOverMenu() =>
            _gameOverMenuView = MenuFactory.Create<GameOverMenuView>(_diContainer);

        private void CreateRewardMenu() =>
            MenuFactory.Create<RewardMenuView>(_diContainer);

        private void CreateGameOverWarningMenuView() =>
            _gameOverWarningMenuView = MenuFactory.Create<GameOverWarningMenuView>(_diContainer);
        
        private void CreatePauseMenu() =>
            _pauseMenuView = MenuFactory.Create<PauseMenuView>(_diContainer);

        private void CreateQuitConfirmMenuView() =>
            _quitConfirmMenuView = MenuFactory.Create<QuitConfirmMenuView>(_diContainer);

        private void ShowQuitConfirmMenu() =>
            _quitConfirmMenuView.Show();

        private void InitHorrorStateMachine() =>
            _horrorStateMachine.ChangeState<PrepareGameState>();

        private void HandleGameState(GameState gameState)
        {
            PlayerEntity localPlayer;
            
            switch (gameState)
            {
                case GameState.QuestsRewarding:
                    // Open reward menu
                    break;
                
                case GameState.KillPlayersOnTheRoad:
                    localPlayer = PlayerEntity.GetLocalPlayer();
                    localPlayer.Kill(PlayerDeathReason.FailedQuests);
                    
                    _gameOverMenuView.Show();
                    break;
                
                case GameState.RestartGame:
                    localPlayer = PlayerEntity.GetLocalPlayer();
                    localPlayer.EnterReviveState();
                    break;
            }
            
            if (_questsSelectionMenuView.IsShown)
                _questsSelectionMenuView.Hide();
            
            if (_locationsSelectionMenuView.IsShown)
                _locationsSelectionMenuView.Hide();
        }

        private void EnterQuitGameplayState() =>
            _gameStateMachine.ChangeState<QuitGameplaySceneState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnOpenPauseMenu()
        {
            if (_pauseMenuView.IsShown)
                return;

            _pauseMenuView.Show();
        }

        private void OnOpenQuestsSelectionMenu()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool canOpenMenu = gameState == GameState.ReadyToLeaveTheRoad;
            
            if (!canOpenMenu)
                return;
            
            if (_questsSelectionMenuView.IsShown)
                _questsSelectionMenuView.Hide();
            else
                _questsSelectionMenuView.Show();
        }

        private void OnOpenLocationsSelectionMenu()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool canOpenMenu = gameState == GameState.ReadyToLeaveTheRoad;
            
            if (!canOpenMenu)
                return;
            
            if (_locationsSelectionMenuView.IsShown)
                _locationsSelectionMenuView.Hide();
            else
                _locationsSelectionMenuView.Show();
        }

        private void OnOpenGameOverWarningMenu()
        {
            if (_gameOverWarningMenuView.IsShown)
                _gameOverWarningMenuView.Hide();
            else
                _gameOverWarningMenuView.Show();
        }

        private void OnContinueClicked() =>
            _pauseMenuView.Hide();

        private void OnQuitClicked() => ShowQuitConfirmMenu();

        private void OnGameOverWarningConfirmClicked() =>
            _gameManagerDecorator.ChangeGameState(GameState.KillPlayersOnTheRoad);

        private void OnGameOverWarningCancelClicked() =>
            _mobileHeadquartersEntity.EnableMainLever();

        private void OnQuitConfirmClicked() => EnterQuitGameplayState();

        private void GameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}