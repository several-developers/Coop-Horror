using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class CageBirdController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CageBirdController(BlindCreatureEntity blindCreatureEntity)
        {
            BlindCreatureAIConfigMeta blindCreatureAIConfig = blindCreatureEntity.GetAIConfig();
            BlindCreatureEntity.References references = blindCreatureEntity.GetReferences();

            _blindCreatureEntity = blindCreatureEntity;
            _cageBirdConfig = blindCreatureAIConfig.GetCageBirdConfig();
            _birdAnimator = references.BirdAnimator;
            _chirpRoutine = new CoroutineHelper(blindCreatureEntity);

            _chirpRoutine.GetRoutineEvent += BirdChirpCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly BlindCreatureEntity _blindCreatureEntity;
        private readonly BlindCreatureAIConfigMeta.CageBirdConfig _cageBirdConfig;
        private readonly Animator _birdAnimator;
        private readonly CoroutineHelper _chirpRoutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TriggerBird(BlindCreatureEntity.BirdReactionType reactionType)
        {
            switch (reactionType)
            {
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

        public void StartChirping() =>
            _chirpRoutine.Start();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ToggleScreaming(bool isScreaming)
        {
            StopSound(BlindCreatureEntity.SFXType.BirdChirp);
            PlaySound(BlindCreatureEntity.SFXType.BirdScream);
            _birdAnimator.SetBool(id: AnimatorHashes.IsScreaming, isScreaming);
        }

        private void Chirp()
        {
            PlaySound(BlindCreatureEntity.SFXType.BirdChirp);
            _birdAnimator.SetTrigger(id: AnimatorHashes.Tweet);
        }

        private void Die()
        {
        }

        private void PlaySound(BlindCreatureEntity.SFXType sfxType)
        {
            if (!IsServer())
                return;

            _blindCreatureEntity.PlaySound(sfxType).Forget();
        }
        
        private void StopSound(BlindCreatureEntity.SFXType sfxType)
        {
            if (!IsServer())
                return;

            _blindCreatureEntity.StopSound(sfxType);
        }

        private IEnumerator BirdChirpCO()
        {
            while (true)
            {
                Vector2 chirpDelay = _cageBirdConfig.ChirpDelay;
                float delay = Random.Range(chirpDelay.x, chirpDelay.y);

                yield return new WaitForSeconds(delay);
                
                Chirp();
            }
        }
        
        private static bool IsServer() =>
            NetworkHorror.IsTrueServer;
    }
}