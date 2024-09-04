using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.CreateLobbyMenu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateLobbyState : IEnterStateAsync, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateLobbyState(IGameStateMachine gameStateMachine, IMenuFactory menuFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;

        private CreateLobbyMenuView _createLobbyMenuView;
        private SaveSelectionMenuView _saveSelectionMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await CreateCreateLobbyMenu();
            await CreateSaveSelectionMenuView();

            _createLobbyMenuView.OnBackButtonClickedEvent += OnBackButtonClicked;
            _createLobbyMenuView.OnStartGameClickedEvent += OnStartGameClicked;
        }

        public void Exit()
        {
            _createLobbyMenuView.OnBackButtonClickedEvent -= OnBackButtonClicked;
            _createLobbyMenuView.OnStartGameClickedEvent -= OnStartGameClicked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask CreateCreateLobbyMenu() =>
            _createLobbyMenuView = await _menuFactory.Create<CreateLobbyMenuView>();

        private async UniTask CreateSaveSelectionMenuView() =>
            _saveSelectionMenuView = await _menuFactory.Create<SaveSelectionMenuView>();

        private void HideSaveSelectionMenu() =>
            _saveSelectionMenuView.Hide();

        private void EnterOnlineMenuState() =>
            _gameStateMachine.ChangeState<OnlineMenuState>();

        private void EnterLoadGameplayState() =>
            _gameStateMachine.ChangeState<LoadGameplayState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnBackButtonClicked()
        {
            HideSaveSelectionMenu();
            EnterOnlineMenuState();
        }

        private void OnStartGameClicked()
        {
            HideSaveSelectionMenu();
            EnterLoadGameplayState();
        }
    }
}