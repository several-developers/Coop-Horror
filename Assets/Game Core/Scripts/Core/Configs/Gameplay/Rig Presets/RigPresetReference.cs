using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items.Rigging;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.RigPresets
{
    [Serializable]
    public class RigPresetReference
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private RigPresetType _presetType;

        [SerializeField, Required]
        private RigPresetMeta _presetMeta;

        // PROPERTIES: ----------------------------------------------------------------------------

        public RigPresetType PresetType => _presetType;
        public RigPresetMeta PresetMeta => _presetMeta;

        private string Label => $"'Preset Type: {_presetType}'";
    }
}