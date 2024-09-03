using System.Collections.Generic;
using GameCore.Gameplay.Items;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.ItemsList
{
    public class ItemsListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ItemMeta[] _itemsMeta;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<ItemMeta> GetAllItems() => _itemsMeta;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
    }
}