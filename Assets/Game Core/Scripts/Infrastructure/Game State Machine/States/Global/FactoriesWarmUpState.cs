using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Gameplay.Factories.Monsters;

namespace GameCore.Infrastructure.StateMachine
{
    public class FactoriesWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FactoriesWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenuFactory menuFactory,
            IEntitiesFactory entitiesFactory,
            IMonstersFactory monstersFactory
        )
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;
            _entitiesFactory = entitiesFactory;
            _monstersFactory = monstersFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;
        private readonly IEntitiesFactory _entitiesFactory;
        private readonly IMonstersFactory _monstersFactory;

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
            await _entitiesFactory.WarmUp();
            await _monstersFactory.WarmUp();
        }
        
        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}