using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Configs.Global.ItemsList;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Utilities;
using GameCore.Infrastructure.Providers.Global;
using GameCore.Infrastructure.Providers.Global.ItemsMeta;
using GameCore.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public class ItemsPreviewFactory : AddressablesFactoryBase<int>, IItemsPreviewFactory, IInitializable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsPreviewFactory(
            DiContainer diContainer,
            IAssetsProvider assetsProvider,
            IDynamicPrefabsLoaderDecorator dynamicPrefabsLoaderDecorator,
            IConfigsProvider configsProvider,
            IItemsMetaProvider itemsMetaProvider,
            PlayerCamera playerCamera
        ) : base(diContainer, assetsProvider, dynamicPrefabsLoaderDecorator)
        {
            _itemsListConfig = configsProvider.GetConfig<ItemsListConfigMeta>();
            _itemsMetaProvider = itemsMetaProvider;
            _cameraReferences = playerCamera.CameraReferences;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ItemsListConfigMeta _itemsListConfig;
        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly CameraReferences _cameraReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Initialize()
        {
            WarmUp().Forget();
        }
        
        public override async UniTask WarmUp() =>
            await SetupAssetsReferences();

        public async UniTaskVoid Create(ulong clientID, int itemID, bool isFirstPerson,
            Action<ItemPreviewObject> callbackEvent)
        {
            bool isItemMetaExists = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaExists)
            {
                LogItemNotFound(itemID);
                return;
            }

            bool isParentFound = TryGetItemParent(out Transform parent);

            if (!isParentFound)
                return;

            var spawnParams = new SpawnParams<ItemPreviewObject>.Builder()
                .SetParent(parent)
                .SetSetupInstanceCallback(SetupItemInstance)
                .SetSuccessCallback(SendSuccessCallback)
                .Build();

            await LoadAndCreateGameObject(itemID, spawnParams);

            // LOCAL METHODS: -----------------------------

            bool TryGetItemParent(out Transform result)
            {
                ItemHandPlacement itemHandPlacement = itemMeta.ItemHandPlacement;

                if (isFirstPerson)
                {
                    result = itemHandPlacement == ItemHandPlacement.Left
                        ? _cameraReferences.LeftHandItemsHolder
                        : _cameraReferences.RightHandItemsHolder;
                }
                else
                {
                    bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

                    if (!isPlayerFound)
                    {
                        result = null;
                        return false;
                    }

                    PlayerReferences playerReferences = playerEntity.References;

                    result = itemHandPlacement == ItemHandPlacement.Left
                        ? playerReferences.LeftHandItemsHolder
                        : playerReferences.RightHandItemsHolder;
                }

                return true;
            }

            void SetupItemInstance(ItemPreviewObject instance)
            {
                ItemMeta.ItemPose itemPose = isFirstPerson ? itemMeta.FpsItemPreview : itemMeta.TpsItemPreview;
                Vector3 position = itemPose.Position;
                Vector3 eulerRotation = itemPose.EulerRotation;
                Vector3 scale = itemPose.Scale;

                Transform itemTransform = instance.transform;
                itemTransform.localPosition = position;
                itemTransform.localRotation = Quaternion.Euler(eulerRotation);
                itemTransform.localScale = scale;
            }

            void SendSuccessCallback(ItemPreviewObject instance) =>
                callbackEvent?.Invoke(instance);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private async UniTask SetupAssetsReferences()
        {
            IEnumerable<ItemsListConfigMeta.ItemReference>
                allItemsReferences = _itemsListConfig.GetAllItemsReferences();

            foreach (ItemsListConfigMeta.ItemReference itemReference in allItemsReferences)
            {
                AssetReferenceGameObject itemPreviewPrefabAsset = itemReference.ItemPreviewPrefabAsset;
                ItemMeta itemMeta = itemReference.ItemMeta;
                int itemID = itemMeta.ItemID;

                await LoadAndReleaseAsset<ItemPreviewObject>(itemPreviewPrefabAsset);
                AddAsset(itemID, itemPreviewPrefabAsset);
            }
        }

        private static void LogItemNotFound(int itemID)
        {
            string log = Log.HandleLog($"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
            Debug.Log(log);
        }
    }
}