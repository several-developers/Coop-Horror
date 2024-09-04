using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Menu;

namespace GameCore.Infrastructure.StateMachine
{
    public class FactoriesWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FactoriesWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenuFactory menuFactory
        )
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await WarmUpFactories();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask WarmUpFactories()
        {
            await _menuFactory.WarmUp();
        }
        
        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}