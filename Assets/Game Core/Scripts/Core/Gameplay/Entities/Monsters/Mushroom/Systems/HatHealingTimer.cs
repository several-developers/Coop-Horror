using System;
using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class HatHealingTimer
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public HatHealingTimer(MushroomEntity mushroomEntity)
        {
            _mushroomEntity = mushroomEntity;
            _timerRoutine = new CoroutineHelper(mushroomEntity);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnTimerEndedEvent = delegate { };
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly CoroutineHelper _timerRoutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartTimer()
        {
            _timerRoutine.GetRoutineEvent += TimerCO;
            _timerRoutine.Start();
        }

        public void StopTimer() =>
            _timerRoutine.Stop();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator TimerCO()
        {
            MushroomAIConfigMeta mushroomAIConfig = _mushroomEntity.GetAIConfig();
            MushroomAIConfigMeta.CommonConfig commonConfig = mushroomAIConfig.GetCommonConfig();
            float hatRegenerationDelay = commonConfig.HatRegenerationDelay;

            yield return new WaitForSeconds(hatRegenerationDelay);
            
            _timerRoutine.GetRoutineEvent -= TimerCO;
            
            OnTimerEndedEvent.Invoke();
        }
    }
}