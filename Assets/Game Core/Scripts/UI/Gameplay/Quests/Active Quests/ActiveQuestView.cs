using System.Collections.Generic;
using GameCore.Gameplay.Items;
using GameCore.Gameplay.Quests;
using GameCore.Infrastructure.Providers.Gameplay.ItemsMeta;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.Quests.ActiveQuests
{
    public class ActiveQuestView : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _titleTMP;

        [SerializeField, Required]
        private ItemInfoView _itemInfoView;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly List<ItemInfoView> _itemsInfoList = new(capacity: 6);

        private QuestRuntimeData _questRuntimeData;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(IItemsMetaProvider itemsMetaProvider, QuestRuntimeData questRuntimeData)
        {
            _questRuntimeData = questRuntimeData;

            UpdateQuestProgress();
            CreateQuestItems(itemsMetaProvider);
        }

        public void UpdateQuestProgress()
        {
            float questProgress = _questRuntimeData.GetQuestProgress();
            questProgress = (int)(questProgress * 100f);

            int questID = _questRuntimeData.QuestID;

            _titleTMP.text = $"Quest #{questID} - <color=#59FF40>{questProgress}%</color>";
        }

        public void UpdateQuestItemsInfo()
        {
            IReadOnlyDictionary<int, QuestItemData> questItems = _questRuntimeData.GetQuestItems();

            foreach (KeyValuePair<int, QuestItemData> pair in questItems)
            {
                int itemID = pair.Key;

                bool isItemInfoViewFound = TryGetItemInfoView(itemID, out ItemInfoView itemInfoView);

                if (!isItemInfoViewFound)
                    continue;

                QuestItemData questItemData = pair.Value;
                int itemsLeft = questItemData.TargetItemQuantity - questItemData.CurrentItemQuantity;
                itemInfoView.UpdateTitle(itemsLeft);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CreateQuestItems(IItemsMetaProvider itemsMetaProvider)
        {
            _itemInfoView.gameObject.SetActive(true);

            IReadOnlyDictionary<int, QuestItemData> questItems = _questRuntimeData.GetQuestItems();

            foreach (KeyValuePair<int, QuestItemData> pair in questItems)
            {
                int itemID = pair.Key;

                bool isItemMetaFound = itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

                if (!isItemMetaFound)
                {
                    Log.PrintError(log: $"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
                    continue;
                }

                QuestItemData questItemData = pair.Value;
                int targetItemQuantity = questItemData.TargetItemQuantity;
                ItemInfoView itemInfoView = Instantiate(_itemInfoView, transform);
                itemInfoView.Setup(itemMeta.Icon, itemMeta.ItemName, targetItemQuantity, itemID);

                _itemsInfoList.Add(itemInfoView);
            }

            _itemInfoView.gameObject.SetActive(false);
        }

        private bool TryGetItemInfoView(int itemID, out ItemInfoView result)
        {
            foreach (ItemInfoView itemInfoView in _itemsInfoList)
            {
                bool isMatches = itemInfoView.ItemID == itemID;

                if (!isMatches)
                    continue;

                result = itemInfoView;
                return true;
            }

            result = null;
            return false;
        }
    }
}