using System;
using System.Collections.Generic;
using CustomEditors;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items;
using Sirenix.OdinInspector;
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
    
    public class QuestsItemsConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private List<ItemsReference> _itemsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}