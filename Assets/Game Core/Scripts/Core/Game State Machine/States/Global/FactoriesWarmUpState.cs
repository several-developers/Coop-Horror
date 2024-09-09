using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Factories.Entities;
using GameCore.Gameplay.Factories.Items;
using GameCore.Gameplay.Factories.Menu;
using GameCore.Gameplay.Factories.Monsters;

namespace GameCore.StateMachine
{
    public class FactoriesWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FactoriesWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenuFactory menuFactory,
            IEntitiesFactory entitiesFactory,
            IMonstersFactory monstersFactory,
            IItemsFactory itemsFactory
        )
        {
            _gameStateMachine = gameStateMachine;
            _menuFactory = menuFactory;
            _entitiesFactory = entitiesFactory;
            _monstersFactory = monstersFactory;
            _itemsFactory = itemsFactory;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenuFactory _menuFactory;
        private readonly IEntitiesFactory _entitiesFactory;
        private readonly IMonstersFactory _monstersFactory;
        private readonly IItemsFactory _itemsFactory;

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
            await _itemsFactory.WarmUp();
        }

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}