using System;
using GameCore.Gameplay.Network;
using Sonity;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.SoundSystem
{
    public class SoundService : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ISoundServiceDecorator soundServiceDecorator)
        {
            _soundServiceDecorator = soundServiceDecorator;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private ISoundServiceDecorator _soundServiceDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _soundServiceDecorator.OnPlaySoundInnerEvent += OnPlaySound;

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _soundServiceDecorator.OnPlaySoundInnerEvent -= OnPlaySound;
        }

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRpc()
        {
            
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnPlaySound(SoundEvent soundEvent, Transform owner)
        {
            
        }
    }
}