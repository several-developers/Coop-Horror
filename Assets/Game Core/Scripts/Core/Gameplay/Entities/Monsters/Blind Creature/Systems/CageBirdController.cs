using GameCore.Gameplay.Network;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class CageBirdController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CageBirdController(BlindCreatureEntity blindCreatureEntity)
        {
            _blindCreatureEntity = blindCreatureEntity;
            _references = blindCreatureEntity.GetReferences();
            _birdAnimator = _references.BirdAnimator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureEntity.References _references;
        private readonly Animator _birdAnimator;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TriggerBird(BlindCreatureEntity.BirdReactionType reactionType)
        {
            switch (reactionType)
            {
                case BlindCreatureEntity.BirdReactionType.Tweet:
                    Tweet();
                    break;

                case BlindCreatureEntity.BirdReactionType.StartScreaming:
                    ToggleScreaming(isScreaming: true);
                    break;

                case BlindCreatureEntity.BirdReactionType.StopScreaming:
                    ToggleScreaming(isScreaming: false);
                    break;

                case BlindCreatureEntity.BirdReactionType.Die:
                    Die();
                    break;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleScreaming(bool isScreaming)
        {
            PlaySound(BlindCreatureEntity.SFXType.BirdScream);
            _birdAnimator.SetBool(id: AnimatorHashes.IsScreaming, isScreaming);
        }

        private void Tweet()
        {
            PlaySound(BlindCreatureEntity.SFXType.BirdTweet);
            _birdAnimator.SetTrigger(id: AnimatorHashes.Tweet);
        }

        private void Die()
        {
        }

        private void PlaySound(BlindCreatureEntity.SFXType sfxType)
        {
            if (!IsServer())
                return;

            _blindCreatureEntity.PlaySound(sfxType);
        }
        
        private void StopSound(BlindCreatureEntity.SFXType sfxType)
        {
            if (!IsServer())
                return;

            _blindCreatureEntity.PlaySound(sfxType);
        }

        private bool IsServer() =>
            NetworkHorror.IsTrueServer;
    }
}