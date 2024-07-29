using CustomEditors;
using GameCore.Gameplay.Entities.Player;
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
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab; // TEMP

        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _footstepsSE;
        
        [SerializeField, Required]
        private SoundEvent _jumpSE;
        
        [SerializeField, Required]
        private SoundEvent _landSE;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float Health => _health;
        public PlayerEntity PlayerPrefab => _playerPrefab;
        
        // SFX
        public SoundEvent FootstepsSE => _footstepsSE;
        public SoundEvent JumpSE => _jumpSE;
        public SoundEvent LandSE => _landSE;

        // FIELDS: --------------------------------------------------------------------------------

        private const string SFXTitle = "SFX";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}
