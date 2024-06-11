using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Items.SpawnSystem
{
    [Serializable]
    public class ItemSpawnConfig
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private ItemMeta _itemMeta;

        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        private int _spawnChance;

        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _firstFloorChance = 25;

        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        [OnValueChanged(nameof(CalculateThirdFloorChance))]
        private int _secondFloorChance = 30;

        [ShowInInspector, Range(0, 100), ReadOnly]
        private int _thirdFloorChance;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ItemMeta ItemMeta => _itemMeta;
        public int SpawnChance => _spawnChance;
        public int FirstFloorChance => _firstFloorChance;
        public int SecondFloorChance => _secondFloorChance;
        
        // ReSharper disable once UnusedMember.Local
        private string Label => $"'Item: {(_itemMeta == null ? "none" : _itemMeta.ItemName)}',   " +
                                $"'Spawn Chance: {_spawnChance}%,   '" +
                                $"'Chances: {_firstFloorChance}% | {_secondFloorChance}% | {_thirdFloorChance}%'";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void CalculateThirdFloorChance() =>
            _thirdFloorChance = 100 - (_firstFloorChance + _secondFloorChance);

        public int GetItemID() =>
            _itemMeta.ItemID;
    }
}