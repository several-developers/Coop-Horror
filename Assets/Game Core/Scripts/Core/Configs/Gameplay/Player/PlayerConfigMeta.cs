using CustomEditors;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Player
{
    public class PlayerConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _health = 100f;

        [SerializeField, Min(0f)]
        private float _defaultHeight = 1.7f;
        
        [SerializeField, Min(0f)]
        private float _defaultRadius = 0.35f;
        
        [SerializeField, Min(0f)]
        private float _sittingHeight = 1f;
        
        [SerializeField, Min(0f)]
        private float _sittingRadius = 0.25f;
        
        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _footstepsSE;
        
        [SerializeField, Required]
        private SoundEvent _jumpSE;
        
        [SerializeField, Required]
        private SoundEvent _landSE;
        
        [SerializeField, Required]
        private SoundEvent _itemPickupSE;
        
        [SerializeField, Required]
        private SoundEvent _itemDropSE;
        
        [SerializeField, Required]
        private SoundEvent _itemSwitchSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float Health => _health;
        
        // SFX
        public SoundEvent FootstepsSE => _footstepsSE;
        public SoundEvent JumpSE => _jumpSE;
        public SoundEvent LandSE => _landSE;
        public SoundEvent ItemPickupSE => _itemPickupSE;
        public SoundEvent ItemDropSE => _itemDropSE;
        public SoundEvent ItemSwitchSE => _itemSwitchSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string SFXTitle = "SFX";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}
