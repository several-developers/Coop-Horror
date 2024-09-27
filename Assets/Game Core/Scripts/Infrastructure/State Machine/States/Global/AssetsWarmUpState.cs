using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Gameplay.Storages.Assets;

namespace GameCore.Infrastructure.StateMachine
{
    public class AssetsWarmUpState : IEnterStateAsync
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AssetsWarmUpState(
            IGameStateMachine gameStateMachine,
            IMenusAssetsStorage menusAssetsStorage,
            IEntitiesAssetsStorage entitiesAssetsStorage,
            IMonstersAssetsStorage monstersAssetsStorage,
            IItemsAssetsStorage itemsAssetsStorage,
            IItemsPreviewAssetsStorage itemsPreviewAssetsStorage,
            IScenesAssetsStorage scenesAssetsStorage
        )
        {
            _gameStateMachine = gameStateMachine;
            _menusAssetsStorage = menusAssetsStorage;
            _entitiesAssetsStorage = entitiesAssetsStorage;
            _monstersAssetsStorage = monstersAssetsStorage;
            _itemsAssetsStorage = itemsAssetsStorage;
            _itemsPreviewAssetsStorage = itemsPreviewAssetsStorage;
            _scenesAssetsStorage = scenesAssetsStorage;

            _gameStateMachine.AddState(this);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IGameStateMachine _gameStateMachine;
        private readonly IMenusAssetsStorage _menusAssetsStorage;
        private readonly IEntitiesAssetsStorage _entitiesAssetsStorage;
        private readonly IMonstersAssetsStorage _monstersAssetsStorage;
        private readonly IItemsAssetsStorage _itemsAssetsStorage;
        private readonly IItemsPreviewAssetsStorage _itemsPreviewAssetsStorage;
        private readonly IScenesAssetsStorage _scenesAssetsStorage;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public async UniTaskVoid Enter()
        {
            await WarmUpAssets();
            EnterLoadMainMenuSceneState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask WarmUpAssets()
        {
            var tasks = new List<UniTask>
            {
                _menusAssetsStorage.WarmUp(),
                _entitiesAssetsStorage.WarmUp(),
                _monstersAssetsStorage.WarmUp(),
                _itemsAssetsStorage.WarmUp(),
                _itemsPreviewAssetsStorage.WarmUp(),
                _scenesAssetsStorage.WarmUp()
            };

            await UniTask.WhenAll(tasks);
        }

        private void EnterLoadMainMenuSceneState() =>
            _gameStateMachine.ChangeState<LoadMainMenuSceneState>();
    }
}