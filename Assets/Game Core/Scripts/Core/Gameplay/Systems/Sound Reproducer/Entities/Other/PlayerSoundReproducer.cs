using GameCore.Configs.Gameplay.Player;
using GameCore.Gameplay.Entities.Player;
using Sonity;

namespace GameCore.Gameplay.Systems.SoundReproducer
{
    public class PlayerSoundReproducer : SoundReproducerBase<PlayerEntity.SFXType>
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        public PlayerSoundReproducer(ISoundProducer<PlayerEntity.SFXType> soundProducer, PlayerConfigMeta playerConfig)
            : base(soundProducer)
        {
            _playerConfig = playerConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly PlayerConfigMeta _playerConfig;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override SoundEvent GetSoundEvent(PlayerEntity.SFXType sfxType)
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

            return soundEvent;
        }
    }
}