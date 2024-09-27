using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Entities
{
    public class OutdoorChestConfigMeta : EntityConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _openSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SoundEvent OpenSE => _openSE;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private const string SFXTitle = "SFX";
    }
}