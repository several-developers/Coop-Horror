using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.Items;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using UnityEngine;

namespace GameCore.Gameplay.Factories.ItemsPreview
{
    public class ItemsPreviewFactory : IItemsPreviewFactory
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ItemsPreviewFactory(IItemsMetaProvider itemsMetaProvider, PlayerCamera playerCamera)
        {
            _itemsMetaProvider = itemsMetaProvider;
            _cameraReferences = playerCamera.CameraReferences;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IItemsMetaProvider _itemsMetaProvider;
        private readonly CameraReferences _cameraReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public bool Create(ulong clientID, int itemID, bool isFirstPerson, out ItemPreviewObject itemPreviewObject)
        {
            itemPreviewObject = null;
            bool isItemMetaExists = _itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

            if (!isItemMetaExists)
            {
                LogItemNotFound(itemID);
                return false;
            }

            ItemPreviewObject itemPreviewPrefab = itemMeta.ItemPreviewPrefab;

            if (itemPreviewPrefab == null)
            {
                LogItemPrefabNotFound(itemID);
                return false;
            }

            ItemHandPlacement itemHandPlacement = itemMeta.ItemHandPlacement;
            Transform parent = null;

            if (isFirstPerson)
            {
                parent = itemHandPlacement == ItemHandPlacement.Left
                    ? _cameraReferences.LeftHandItemsHolder
                    : _cameraReferences.RightHandItemsHolder;
            }
            else
            {
                bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

                if (!isPlayerFound)
                    return false;

                PlayerReferences playerReferences = playerEntity.References;

                parent = itemHandPlacement == ItemHandPlacement.Left
                    ? playerReferences.LeftHandItemsHolder
                    : playerReferences.RightHandItemsHolder;
            }

            itemPreviewObject = Object.Instantiate(itemPreviewPrefab, parent);

            ItemMeta.ItemPose itemPose = isFirstPerson ? itemMeta.FpsItemPreview : itemMeta.TpsItemPreview;
            Vector3 position = itemPose.Position;
            Vector3 eulerRotation = itemPose.EulerRotation;
            Vector3 scale = itemPose.Scale;

            Transform itemTransform = itemPreviewObject.transform;
            itemTransform.localPosition = position;
            itemTransform.localRotation = Quaternion.Euler(eulerRotation);
            itemTransform.localScale = scale;

            if (isFirstPerson)
                itemPreviewObject.ChangeLayer();

            return true;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private static void LogItemNotFound(int itemID)
        {
            string log = Log.HandleLog($"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
            Debug.Log(log);
        }

        private static void LogItemPrefabNotFound(int itemID)
        {
            string log = Log.HandleLog($"Item Prefab with ID <gb>({itemID})</gb> <rb>not found</rb>!");
            Debug.Log(log);
        }
    }
}