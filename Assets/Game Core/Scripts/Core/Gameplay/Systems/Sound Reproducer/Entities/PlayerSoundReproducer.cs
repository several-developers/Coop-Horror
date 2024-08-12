using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.Entities.Player;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class PlayerSoundReproducer : SoundReproducerBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        public PlayerSoundReproducer(Transform owner, PlayerConfigMeta playerConfig) : base(owner) =>
            _playerConfig = playerConfig;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerConfigMeta _playerConfig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlaySound(PlayerEntity.SFXType sfxType)
        {
            SoundEvent soundEvent = sfxType switch
            {
                PlayerEntity.SFXType.Footsteps => _playerConfig.FootstepsSE,
                PlayerEntity.SFXType.Jump => _playerConfig.JumpSE,
                PlayerEntity.SFXType.Land => _playerConfig.LandSE,
                PlayerEntity.SFXType.ItemPickup => _playerConfig.ItemPickupSE,
                PlayerEntity.SFXType.ItemDrop => _playerConfig.ItemDropSE,
                PlayerEntity.SFXType.ItemSwitch => _playerConfig.ItemSwitchSE,
                _ => null
            };

            if (soundEvent == null)
                return;
            
            PlaySound(soundEvent);
        }
    }
}