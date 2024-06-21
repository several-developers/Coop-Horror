using System.Collections;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.GoodClown
{
    public class TransformationSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TransformationSystem(GoodClownEntity goodClownEntity)
        {
            _goodClownEntity = goodClownEntity;
            _goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _transform = goodClownEntity.transform;

            GoodClownAIConfigMeta goodClownAIConfig = goodClownEntity.GetGoodClownAIConfig();
            _transformationConfig = goodClownAIConfig.TransformationConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly GoodClownEntity _goodClownEntity;
        private readonly GoodClownAIConfigMeta _goodClownAIConfig;
        private readonly GoodClownAIConfigMeta.TransformationSettings _transformationConfig;
        private readonly Transform _transform;

        private Coroutine _transformationCheckCO;
        private bool _isTransformationDistanceValid;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            StartTransformationCheck();
        }

        public void Stop()
        {
            StopTransformationCheck();
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckTransformation()
        {
            PlayerEntity targetPlayer = _goodClownEntity.GetTargetPlayer();
            bool isTargetFound = targetPlayer != null;

            if (!isTargetFound)
                return;

            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 thisPosition = _transform.position;
            float distanceToTarget = Vector3.Distance(a: targetPosition, b: thisPosition);
            
            _isTransformationDistanceValid = distanceToTarget <= _transformationConfig.CanTransformAtDistance;

            if (!_isTransformationDistanceValid)
                return;
            
            
        }

        private void StartTransformationCheck()
        {
            IEnumerator routine = TransformationCheckCO();
            _transformationCheckCO = _goodClownEntity.StartCoroutine(routine);
        }

        private void StopTransformationCheck()
        {
            if (_transformationCheckCO == null)
                return;
            
            _goodClownEntity.StopCoroutine(_transformationCheckCO);
        }
        
        private IEnumerator TransformationCheckCO()
        {
            while (true)
            {
                float checkInterval = _transformationConfig.TransformationCheckInterval;
                yield return new WaitForSeconds(checkInterval);

                CheckTransformation();
            }
        }
    }
}