using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Systems.Utilities;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom
{
    public class WhisperingSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WhisperingSystem(MushroomEntity mushroomEntity)
        {
            _mushroomEntity = mushroomEntity;
            _cycleRoutine = new CoroutineHelper(mushroomEntity);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly MushroomEntity _mushroomEntity;
        private readonly CoroutineHelper _cycleRoutine;

        private bool _isPaused;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            _cycleRoutine.GetRoutineEvent += CycleCO;
            _cycleRoutine.Start();
        }

        public void Pause() =>
            _isPaused = true;

        public void Unpause() =>
            _isPaused = false;

        public void Stop()
        {
            _cycleRoutine.GetRoutineEvent -= CycleCO;
            _cycleRoutine.Stop();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayWhisperingSound()
        {
            if (_isPaused)
                return;
            
            _mushroomEntity.PlaySound(MushroomEntity.SFXType.Whispering).Forget();
        }

        private IEnumerator CycleCO()
        {
            MushroomAIConfigMeta mushroomAIConfig = _mushroomEntity.GetAIConfig();
            MushroomAIConfigMeta.CommonConfig commonConfig = mushroomAIConfig.GetCommonConfig();

            while (true)
            {
                Vector2 whisperingInterval = commonConfig.WhisperingInterval;
                float interval = Random.Range(whisperingInterval.x, whisperingInterval.y);
                
                yield return new WaitForSeconds(interval);
                
                PlayWhisperingSound();
            }
        }
    }
}