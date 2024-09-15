using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class IdleState : IEnterState, IExitState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public IdleState(MushroomEntity mushroomEntity)
        {
            MushroomAIConfigMeta mushroomAIConfig = mushroomEntity.GetAIConfig();

            _mushroomEntity = mushroomEntity;
            _wanderingConfig = mushroomAIConfig.GetWanderingConfig();
            _whisperingSystem = mushroomEntity.GetWhisperingSystem();
            _wanderingTimerRoutine = new CoroutineHelper(mushroomEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomAIConfigMeta.WanderingConfig _wanderingConfig;
        private readonly WhisperingSystem _whisperingSystem;
        private readonly CoroutineHelper _wanderingTimerRoutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            _wanderingTimerRoutine.GetRoutineEvent += WanderingTimerCO;

            bool isHatDamaged = _mushroomEntity.IsHatDamaged;
            MushroomEntity.Emotion emotion = isHatDamaged ? MushroomEntity.Emotion.Angry : MushroomEntity.Emotion.Happy;
            _mushroomEntity.SetEmotion(emotion);
            
            UnpauseWhisperingSystem();
            _mushroomEntity.DisableAgent();
            _wanderingTimerRoutine.Start();
        }

        public void Exit()
        {
            _wanderingTimerRoutine.GetRoutineEvent -= WanderingTimerCO;

            _wanderingTimerRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UnpauseWhisperingSystem() =>
            _whisperingSystem.Unpause();

        private void EnterWanderingState() =>
            _mushroomEntity.EnterWanderingState();

        private IEnumerator WanderingTimerCO()
        {
            float minDelay = _wanderingConfig.MinDelay;
            float maxDelay = _wanderingConfig.MaxDelay;
            float timeBeforeWandering = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(timeBeforeWandering);

            EnterWanderingState();
        }
    }
}