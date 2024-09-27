using System.Collections.Generic;
using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.RigPresets
{
    public class RigPresetsConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        [ListDrawerSettings(ListElementLabelName = "Label")]
        private List<RigPresetReference> _presetsReferences;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<RigPresetReference> GetAllPresetsReferences() => _presetsReferences;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}