using System.Collections.Generic;
using CustomEditors;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Quests
{
    public class QuestsConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Range(1, 5)]
        private int _maxQuestsAmount = 5;

        [SerializeField, Range(1, 5)]
        private int _maxActiveQuests = 3;

        [SerializeField]
        [InfoBox(message: Warning, InfoMessageType.Error, nameof(_ignoreMobileHQQuestsCheck))]
        private bool _ignoreMobileHQQuestsCheck;

        [SerializeField, Space(height: 5)]
        private QuestDifficulty[] _questsLookUpList;

        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private QuestPresetConfig[] _questsPresets;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int MaxQuestsAmount => _maxQuestsAmount;
        public int MaxActiveQuests => _maxActiveQuests;
        public bool IgnoreMobileHQQuestsCheck => _ignoreMobileHQQuestsCheck;

        // FIELDS: --------------------------------------------------------------------------------

        private const string Warning = "Warning! Must be disabled for the correct gameplay!";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyCollection<QuestDifficulty> GetQuestsLookUpList() => _questsLookUpList;

        public IEnumerable<QuestPresetConfig> GetAllQuestsPresets() => _questsPresets;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}