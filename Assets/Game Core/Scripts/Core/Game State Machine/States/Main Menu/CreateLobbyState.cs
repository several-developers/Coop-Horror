using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.StateMachine
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

        private SaveSelectionMenuView _saveSelectionMenuView;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await CreateSaveSelectionMenuView();
        }

        public void Exit()
        {
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask CreateSaveSelectionMenuView() =>
            _saveSelectionMenuView = await _menuFactory.Create<SaveSelectionMenuView>();
    }
}