using System.Collections.Generic;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Visual
{
    public class VisualConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        [ListDrawerSettings]
        private List<VisualPresetMeta> _presetsConfigs;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IEnumerable<VisualPresetMeta> GetAllPresetsConfigs() => _presetsConfigs;

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}