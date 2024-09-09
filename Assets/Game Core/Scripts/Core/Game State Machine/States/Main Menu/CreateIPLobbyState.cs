using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.LobbiesMenu.IPLobby;

namespace GameCore.StateMachine
{
    public class CreateIPLobbyState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateIPLobbyState(IGameStateMachine gameStateMachine, IMenusFactory menusFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menusFactory = menusFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenusFactory _menusFactory;

        private IPLobbyMenuView _ipLobbyMenu;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter() =>
            await CreateIPLobbyMenu();

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private async UniTask CreateIPLobbyMenu()
        {
            _ipLobbyMenu = await _menusFactory.Create<IPLobbyMenuView>();

            _ipLobbyMenu.OnCloseClickedEvent += OnCloseClicked;
        }

        private void EnterMainMenuState() =>
            _gameStateMachine.ChangeState<MainMenuState>();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnCloseClicked() => EnterMainMenuState();
    }
}