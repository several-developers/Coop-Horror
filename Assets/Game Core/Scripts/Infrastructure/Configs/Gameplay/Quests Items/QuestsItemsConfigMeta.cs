using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.QuestsItems
{
    public class QuestsItemsConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private List<ItemsReference> _itemsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public ItemsReference GetItemsReference(QuestDifficulty difficulty)
        {
            foreach (ItemsReference itemsReference in _itemsReferences)
            {
                bool isMatches = itemsReference.QuestDifficulty == difficulty;

                if (isMatches)
                    return itemsReference;
            }

            Log.PrintError(log: $"Items Reference with difficulty <gb>{difficulty}</gb> <rb>not found</rb>!");
            return null;
        }

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}