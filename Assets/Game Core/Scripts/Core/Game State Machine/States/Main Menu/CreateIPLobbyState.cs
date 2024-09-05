using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.LobbiesMenu.IPLobby;

namespace GameCore.StateMachine
{
    public class CreateIPLobbyState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateIPLobbyState(IGameStateMachine gameStateMachine, IMenuFactory menuFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;

        private IPLobbyMenuView _ipLobbyMenu;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter() =>
            await CreateIPLobbyMenu();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private async UniTask CreateIPLobbyMenu()
        {
            _ipLobbyMenu = await _menuFactory.Create<IPLobbyMenuView>();

            _ipLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();
    }
}