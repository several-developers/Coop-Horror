using System.Collections.Generic;
using GameCore.Infrastructure.Configs.Gameplay.RigPresets;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Items.Rigging;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;

namespace GameCore.Infrastructure.Providers.Gameplay.RigPresets
{
    public class RigPresetsProvider : IRigPresetsProvider
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public RigPresetsProvider(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _presets = new Dictionary<RigPresetType, RigPresetMeta>();

            SetupPresetsDictionary(gameplayConfigsProvider);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly Dictionary<RigPresetType, RigPresetMeta> _presets;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public bool TryGetPresetMeta(RigPresetType presetType, out RigPresetMeta presetMeta) =>
            _presets.TryGetValue(presetType, out presetMeta);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPresetsDictionary(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            var rigPresetsConfig = gameplayConfigsProvider.GetConfig<RigPresetsConfigMeta>();
            IEnumerable<RigPresetReference> allPresetsReferences = rigPresetsConfig.GetAllPresetsReferences();

            foreach (RigPresetReference presetReference in allPresetsReferences)
            {
                RigPresetType presetType = presetReference.PresetType;
                bool isSuccessfullyAdded = _presets.TryAdd(presetType, presetReference.PresetMeta);

                if (isSuccessfullyAdded)
                    continue;

                Log.PrintError(log: $"Rig Preset of type <gb>{presetType}</gb> is <rb>already added</rb>!");
            }
        }
    }
}