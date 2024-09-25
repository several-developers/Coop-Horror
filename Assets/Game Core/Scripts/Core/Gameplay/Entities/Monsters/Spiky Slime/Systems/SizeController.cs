using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class SizeController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SizeController(SpikySlimeEntity spikySlimeEntity)
        {
            _spikySlimeEntity = spikySlimeEntity;
            _spikySlimeAIConfig = spikySlimeEntity.GetAIConfig();
            _references = spikySlimeEntity.GetReferences();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly SpikySlimeEntity _spikySlimeEntity;
        private readonly SpikySlimeAIConfigMeta _spikySlimeAIConfig;
        private readonly SpikySlimeReferences _references;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ChangeSize(float slimeScale)
        {
            Transform modelPivot = _references.ModelPivot;
            Transform anchorPoints = _references.AnchorPoints;
            Light slimeLight = _references.SlimeLight;
            SphereCollider slimeTrigger = _references.SlimeTrigger;
            
            Vector3 triggerCenter = slimeTrigger.center;
            triggerCenter.y *= slimeScale;

            modelPivot.localScale *= slimeScale;
            anchorPoints.localScale *= slimeScale;
            slimeLight.range *= slimeScale;
            slimeTrigger.radius *= slimeScale;
            slimeTrigger.center = triggerCenter;
        }

        public float GetRandomSlimeScale()
        {
            EntityLocation entityLocation = _spikySlimeEntity.GetCurrentLocation();
            SpikySlimeAIConfigMeta.SizeConfig sizeConfig = _spikySlimeAIConfig.GetRandomSizeConfig(entityLocation);
            Vector2 scaleRange = sizeConfig.Scale;
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            return scale;
        }
    }
}