using System;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Configs.Gameplay.Quests
{
    [Serializable]
    public class QuestPresetConfig
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private QuestDifficulty _difficulty;

        [SerializeField, MinMaxSlider(minValue: 1, maxValue: 6, showFields: true)]
        private Vector2Int _itemsListLength = new(x: 1, y: 6);

        [SerializeField, MinMaxSlider(minValue: 1, maxValue: 4, showFields: true)]
        private Vector2Int _itemQuantity = new(x: 1, y: 4);

        [SerializeField, MinMaxSlider(minValue: 1, maxValue: 4, showFields: true)]
        private Vector2Int _deadline = new(x: 1, y: 4);

        // PROPERTIES: ----------------------------------------------------------------------------

        public QuestDifficulty Difficulty => _difficulty;

        public int GetRandomItemsListLength() =>
            Random.Range(_itemsListLength.x, _itemsListLength.y + 1);

        public int GetRandomItemQuantity() =>
            Random.Range(_itemQuantity.x, _itemQuantity.y + 1);

        public int GetRandomDeadline() =>
            Random.Range(_deadline.x, _deadline.y + 1);

        private string Label =>
            $"'Difficulty: {_difficulty}',   'Items list length: {_itemsListLength}',   'Item quantity: {_itemQuantity}',   'Deadline: {_deadline}'";
    }
}