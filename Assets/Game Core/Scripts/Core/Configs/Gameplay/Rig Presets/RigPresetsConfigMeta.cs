using System.Collections.Generic;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.RigPresets
{
    public class RigPresetsConfigMeta : EditorMeta
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
    }
}