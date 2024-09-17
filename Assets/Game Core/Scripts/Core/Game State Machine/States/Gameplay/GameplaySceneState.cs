using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.HorrorStateMachineSpace;
using GameCore.Gameplay.Systems.InputManagement;
using GameCore.Infrastructure.Providers.Global;
using GameCore.UI.Gameplay.Chat;
using GameCore.UI.Gameplay.GameMap;
using GameCore.UI.Gameplay.GameOverMenu;
using GameCore.UI.Gameplay.GameOverWarningMenu;
using GameCore.UI.Gameplay.PauseMenu;
using GameCore.UI.Gameplay.Quests;
using GameCore.UI.Gameplay.Quests.ActiveQuests;
using GameCore.UI.Gameplay.RewardMenu;
using GameCore.Utilities;
using Zenject;

namespace GameCore.StateMachine
{
    public class GameplaySceneState : IEnterStateAsync, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public GameplaySceneState(
            IGameStateMachine gameStateMachine,
            IHorrorStateMachine horrorStateMachine,
            IMenusFactory menusFactory,
            ITrainEntity trainEntity,
            IConfigsProvider configsProvider,
            IGameManagerDecorator gameManagerDecorator,
            DiContainer diContainer
        )
        {
            _gameStateMachine = gameStateMachine;
            _horrorStateMachine = horrorStateMachine;
            _menusFactory = menusFactory;
            _trainEntity = trainEntity;
            _gameManagerDecorator = gameManagerDecorator;
            _diContainer = diContainer;
            _inputReader = configsProvider.GetInputReader();

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IHorrorStateMachine _horrorStateMachine;
        private readonly IMenusFactory _menusFactory;
        private readonly ITrainEntity _trainEntity;
        private readonly IGameManagerDecorator _gameManagerDecorator;
        private readonly DiContainer _diContainer;
        private readonly InputReader _inputReader;

        private PauseMenuView _pauseMenuView;
        private QuitConfirmMenuView _quitConfirmMenuView;
        private QuestsSelectionMenuView _questsSelectionMenuView;
        private GameMapUI _gameMapUI;
        private GameOverMenuView _gameOverMenuView;
        private GameOverWarningMenuView _gameOverWarningMenuView;
        private ChatMenuUI _chatMenuUI;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            LockCursor();
            EnableGameplayInput();

            await CreateChatMenu();
            await CreateActiveQuestsView(); // TEMP
            await CreateQuestsSelectionMenuView(); // TEMP
            await CreateGameMapMenu(); // TEMP
            await CreateGameOverMenu(); // TEMP
            await CreateRewardMenu(); // TEMP
            await CreateGameOverWarningMenuView(); // TEMP
            await CreatePauseMenu(); // TEMP
            await CreateQuitConfirmMenuView(); // TEMP

            InitHorrorStateMachine();

            _inputReader.OnPauseEvent += OnOpenPauseMenu;
            _inputReader.OnOpenChatEvent += OnOpenChatMenu;
            _inputReader.OnSubmitEvent += OnSendChatMessage;

            _trainEntity.OnOpenQuestsSelectionMenuEvent += OnOpenQuestsSelectionMenu;
            _trainEntity.OnOpenGameOverWarningMenuEvent += OnOpenGameOverWarningMenu;
            _trainEntity.OnOpenGameMapEvent += OnOpenGameMap;

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
            _inputReader.OnOpenChatEvent -= OnOpenChatMenu;
            _inputReader.OnSubmitEvent -= OnSendChatMessage;

            _trainEntity.OnOpenQuestsSelectionMenuEvent -= OnOpenQuestsSelectionMenu;
            _trainEntity.OnOpenGameOverWarningMenuEvent -= OnOpenGameOverWarningMenu;
            _trainEntity.OnOpenGameMapEvent -= OnOpenGameMap;

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

        private async UniTask CreateChatMenu() =>
            _chatMenuUI = await _menusFactory.Create<ChatMenuUI>(_diContainer);

        private async UniTask CreateActiveQuestsView() =>
            await _menusFactory.Create<ActiveQuestsView>(_diContainer);

        private async UniTask CreateQuestsSelectionMenuView() =>
            _questsSelectionMenuView = await _menusFactory.Create<QuestsSelectionMenuView>(_diContainer);

        private async UniTask CreateGameMapMenu() =>
            _gameMapUI = await _menusFactory.Create<GameMapUI>(_diContainer);

        private async UniTask CreateGameOverMenu() =>
            _gameOverMenuView = await _menusFactory.Create<GameOverMenuView>(_diContainer);

        private async UniTask CreateRewardMenu() =>
            await _menusFactory.Create<RewardMenuView>(_diContainer);

        private async UniTask CreateGameOverWarningMenuView() =>
            _gameOverWarningMenuView = await _menusFactory.Create<GameOverWarningMenuView>(_diContainer);

        private async UniTask CreatePauseMenu() =>
            _pauseMenuView = await _menusFactory.Create<PauseMenuView>(_diContainer);

        private async UniTask CreateQuitConfirmMenuView() =>
            _quitConfirmMenuView = await _menusFactory.Create<QuitConfirmMenuView>(_diContainer);

        private void ShowQuitConfirmMenu() =>
            _quitConfirmMenuView.Show();

        private void InitHorrorStateMachine() =>
            _horrorStateMachine.ChangeState<PrepareGameState>();

#warning СЛОМАНО, СРОЧНО ПОЧИНИТЬ
        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.GameOver:
                    _gameOverMenuView.Show();
                    break;

                case GameState.QuestsRewarding:
                    // Open reward menu
                    break;

                // case GameState.KillPlayersOnTheRoad:
                //     PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
                //     localPlayer.Kill(PlayerDeathReason.FailedQuests);
                //     break;

                case GameState.RestartGame:
                    _gameOverMenuView.Hide();
                    break;
            }

            if (_questsSelectionMenuView.IsShown)
                _questsSelectionMenuView.Hide();
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

        private void OnOpenChatMenu()
        {
            if (_chatMenuUI.IsShown)
                return;

            _chatMenuUI.ActivateChat();
            _chatMenuUI.Show();
        }

        private void OnSendChatMessage()
        {
            if (!_chatMenuUI.IsShown)
                return;

            _chatMenuUI.TrySendChatMessage();
        }

        private void OnOpenGameMap()
        {
            if (_chatMenuUI.IsShown)
                return;

            _gameMapUI.Show();
        }

        private void OnOpenQuestsSelectionMenu()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool canOpenMenu = gameState == GameState.Gameplay;

            if (!canOpenMenu)
                return;

            if (_questsSelectionMenuView.IsShown)
                _questsSelectionMenuView.Hide();
            else
                _questsSelectionMenuView.Show();
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
            _gameManagerDecorator.ChangeGameState(GameState.KillPlayersByMetroMonster);

        private void OnGameOverWarningCancelClicked()
        {
            //_trainEntity.EnableMainLever();
        }

        private void OnQuitConfirmClicked() => EnterQuitGameplayState();

        private void GameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}