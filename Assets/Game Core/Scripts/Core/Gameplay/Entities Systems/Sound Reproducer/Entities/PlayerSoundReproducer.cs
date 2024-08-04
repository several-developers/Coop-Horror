using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.Entities.Player;
using Sonity;
using UnityEngine;

namespace GameCore.Gameplay.EntitiesSystems.SoundReproducer
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
            SoundEvent soundEvent = null;
            
            switch (sfxType)
            {
                case PlayerEntity.SFXType.Footsteps:
                    soundEvent = _playerConfig.FootstepsSE;
                    break;
                
                case PlayerEntity.SFXType.Jump:
                    soundEvent = _playerConfig.JumpSE;
                    break;
                
                case PlayerEntity.SFXType.Land:
                    soundEvent = _playerConfig.LandSE;
                    break;
            }

            if (soundEvent == null)
                return;
            
            PlaySound(soundEvent);
        }
    }
}