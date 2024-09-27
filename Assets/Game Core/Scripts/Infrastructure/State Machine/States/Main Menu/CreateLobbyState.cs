using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;
using GameCore.UI.MainMenu.SaveSelectionMenu;

namespace GameCore.Infrastructure.StateMachine
{
    public class CreateLobbyState : IEnterStateAsync, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CreateLobbyState(IGameStateMachine gameStateMachine, IMenusFactory menusFactory)
        {
            _gameStateMachine = gameStateMachine;
            _menusFactory = menusFactory;
            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenusFactory _menusFactory;

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
            _saveSelectionMenuView = await _menusFactory.Create<SaveSelectionMenuView>();
    }
}