using Cysharp.Threading.Tasks;
using GameCore.Gameplay.AssetsStorages;

namespace GameCore.StateMachine
{
    public class FactoriesWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public FactoriesWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenusAssetsStorage menusAssetsStorage,
            IEntitiesAssetsStorage entitiesAssetsStorage,
            IMonstersAssetsStorage monstersAssetsStorage,
            IItemsAssetsStorage itemsAssetsStorage,
            IItemsPreviewAssetsStorage itemsPreviewAssetsStorage
        )
        {
            _gameStateMachine = gameStateMachine;
            _menusAssetsStorage = menusAssetsStorage;
            _entitiesAssetsStorage = entitiesAssetsStorage;
            _monstersAssetsStorage = monstersAssetsStorage;
            _itemsAssetsStorage = itemsAssetsStorage;
            _itemsPreviewAssetsStorage = itemsPreviewAssetsStorage;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenusAssetsStorage _menusAssetsStorage;
        private readonly IEntitiesAssetsStorage _entitiesAssetsStorage;
        private readonly IMonstersAssetsStorage _monstersAssetsStorage;
        private readonly IItemsAssetsStorage _itemsAssetsStorage;
        private readonly IItemsPreviewAssetsStorage _itemsPreviewAssetsStorage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await WarmUpFactories();
            EnterLoadMainMenuState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask WarmUpFactories()
        {
            await _menusAssetsStorage.WarmUp();
            await _entitiesAssetsStorage.WarmUp();
            await _monstersAssetsStorage.WarmUp();
            await _itemsAssetsStorage.WarmUp();
            await _itemsPreviewAssetsStorage.WarmUp();
        }

        private void EnterLoadMainMenuState() =>
            _gameStateMachine.ChangeState<LoadMainMenuState>();
    }
}