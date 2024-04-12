using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items;
using UnityEngine;

namespace GameCore.Configs.Gameplay.QuestsItems
{
    [Serializable]
    public class ItemsReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private QuestDifficulty _questDifficulty;

        [SerializeField, Space(height: 5)]
        private List<ItemMeta> _itemsMeta;

        // PROPERTIES: ----------------------------------------------------------------------------

        public QuestDifficulty QuestDifficulty => _questDifficulty;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<ItemMeta> GetAllItemsMeta() => _itemsMeta;
    }
}