using System;
using System.Collections.Generic;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Items.Generators.OutdoorChest
{
    [Serializable]
    public class OutdoorChestsItemsSpawnConfig
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public OutdoorChestsItemsSpawnConfig()
        {
            _chestsAmountConfigs = new List<AmountConfig>();
            _itemsAmountInChestConfigs = new List<AmountConfig>();
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        private int _questsItemsSpawnChance = 30;

        [SerializeField]
        private List<AmountConfig> _chestsAmountConfigs;

        [SerializeField]
        private List<AmountConfig> _itemsAmountInChestConfigs;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int QuestsItemsSpawnChance => _questsItemsSpawnChance;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public int GetRandomChestsAmount()
        {
            int chestsConfigsAmount = _chestsAmountConfigs.Count;
            
            if (chestsConfigsAmount == 0)
                return 0;

            var chances = new double[chestsConfigsAmount];

            for (int i = 0; i < chestsConfigsAmount; i++)
                chances[i] = _chestsAmountConfigs[i].Chance;

            int randomIndex = GlobalUtilities.GetRandomIndex(chances);
            int chestsAmount = _chestsAmountConfigs[randomIndex].Amount;
            return chestsAmount;
        }
        
        public int GetItemsRandomAmount()
        {
            int itemsConfigsAmount = _itemsAmountInChestConfigs.Count;
            
            if (itemsConfigsAmount == 0)
                return 0;

            var chances = new double[itemsConfigsAmount];

            for (int i = 0; i < itemsConfigsAmount; i++)
                chances[i] = _itemsAmountInChestConfigs[i].Chance;

            int randomIndex = GlobalUtilities.GetRandomIndex(chances);
            int itemsAmount = _itemsAmountInChestConfigs[randomIndex].Amount;
            return itemsAmount;
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        [Serializable]
        public struct AmountConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(0)]
            private int _amount;

            [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
            private int _chance;

            // PROPERTIES: ----------------------------------------------------------------------------

            public int Amount => _amount;
            public int Chance => _chance;
        }
    }
}