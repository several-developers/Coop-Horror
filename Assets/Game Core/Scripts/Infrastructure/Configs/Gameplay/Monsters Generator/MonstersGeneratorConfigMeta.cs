using System;
using System.Collections.Generic;
using GameCore.InfrastructureTools.Configs;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.MonstersGenerator
{
    public class MonstersGeneratorConfigMeta : ConfigMeta
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MonstersGeneratorConfigMeta() =>
            _amountConfigs = new List<AmountConfig>();

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<AmountConfig> _amountConfigs;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;

        public int GetRandomMonstersAmount(int playersAmount)
        {
            AmountConfig amountConfig = GetAmountConfig(playersAmount);
            int monstersAmount = GetRandomMonstersAmount(amountConfig);
            return monstersAmount;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private AmountConfig GetAmountConfig(int playersAmount)
        {
            foreach (AmountConfig amountConfig in _amountConfigs)
            {
                bool isMatches = amountConfig.PlayersAmount == playersAmount;

                if (!isMatches)
                    continue;

                return amountConfig;
            }
            
            return _amountConfigs[^1]; // Get the last config.
        }
        
        private int GetRandomMonstersAmount(AmountConfig amountConfig)
        {
            var chances = new double[]
            {
                amountConfig.OneMonsterChance,
                amountConfig.TwoMonstersChance,
                amountConfig.ThreeMonstersChance,
                amountConfig.FourMonstersChance
            };

            int monstersAmount = GlobalUtilities.GetRandomIndex(chances) + 1;
            return monstersAmount;
        }
        
        // INNER CLASSES: -------------------------------------------------------------------------
        
        [Serializable]
        public class AmountConfig
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField, Min(1)]
            private int _playersAmount = 1;

            [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
            private int _oneMonsterChance;
            
            [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
            private int _twoMonstersChance;
            
            [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
            private int _threeMonstersChance;
            
            [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
            private int _fourMonstersChance;

            // PROPERTIES: ----------------------------------------------------------------------------

            public int PlayersAmount => _playersAmount;
            public int OneMonsterChance => _oneMonsterChance;
            public int TwoMonstersChance => _twoMonstersChance;
            public int ThreeMonstersChance => _threeMonstersChance;
            public int FourMonstersChance => _fourMonstersChance;
            
            private string Label => $"'Players: {_playersAmount}',   " +
                                    $"'1: {_oneMonsterChance}%',   " +
                                    $"'2: {_twoMonstersChance}%',   " +
                                    $"'3: {_threeMonstersChance}%',   " +
                                    $"'4: {_fourMonstersChance}%'";
        }
    }
}