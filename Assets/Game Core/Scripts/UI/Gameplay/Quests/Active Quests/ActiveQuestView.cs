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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(IItemsMetaProvider itemsMetaProvider, QuestRuntimeData questRuntimeData)
        {
            _titleTMP.text = $"Quest #{questRuntimeData.QuestID}";
            _itemInfoView.gameObject.SetActive(true);

            IReadOnlyDictionary<int, int> questItems = questRuntimeData.GetQuestItems();

            foreach (KeyValuePair<int, int> pair in questItems)
            {
                int itemID = pair.Key;

                bool isItemMetaFound = itemsMetaProvider.TryGetItemMeta(itemID, out ItemMeta itemMeta);

                if (!isItemMetaFound)
                {
                    Log.PrintError(log: $"Item with ID <gb>({itemID})</gb> <rb>not found</rb>!");
                    continue;
                }

                int itemQuantity = pair.Value;
                ItemInfoView itemInfoView = Instantiate(_itemInfoView, transform);
                itemInfoView.Setup(itemMeta.Icon, itemMeta.ItemName, itemQuantity);

                _itemsInfoList.Add(itemInfoView);
            }

            _itemInfoView.gameObject.SetActive(false);
        }
    }
}