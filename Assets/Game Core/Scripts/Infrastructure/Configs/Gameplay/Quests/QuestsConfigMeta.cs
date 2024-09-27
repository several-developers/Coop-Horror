using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Quests
{
    public class QuestsConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: Constants.Settings)]
        [BoxGroup(Constants.SettingsIn, showLabel: false), SerializeField, Range(1, 5)]
        private int _maxQuestsAmount = 5;

        [BoxGroup(Constants.SettingsIn), SerializeField, Range(1, 5)]
        private int _maxActiveQuests = 3;

        [BoxGroup(Constants.SettingsIn), SerializeField]
        [InfoBox(message: Warning, InfoMessageType.Error, nameof(_ignoreTrainQuestsCheck))]
        private bool _ignoreTrainQuestsCheck;

        [SerializeField, Space(height: 5)]
        private QuestDifficulty[] _questsLookUpList;

        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private QuestPresetConfig[] _questsPresets;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int MaxQuestsAmount => _maxQuestsAmount;
        public int MaxActiveQuests => _maxActiveQuests;
        public bool IgnoreTrainQuestsCheck => _ignoreTrainQuestsCheck;

        // FIELDS: --------------------------------------------------------------------------------

        private const string Warning = "Warning! Must be disabled for the correct gameplay!";

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyCollection<QuestDifficulty> GetQuestsLookUpList() => _questsLookUpList;

        public IEnumerable<QuestPresetConfig> GetAllQuestsPresets() => _questsPresets;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}