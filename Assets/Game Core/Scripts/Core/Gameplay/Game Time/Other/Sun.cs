using GameCore.Configs.Gameplay.Time;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class Sun : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle, IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _timeCycle = timeCycle;
            _timeConfig = gameplayConfigsProvider.GetTimeConfig();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Light _light;
        
        // FIELDS: --------------------------------------------------------------------------------

        private ITimeCycle _timeCycle;
        private TimeConfigMeta _timeConfig;
        private Transform _transform;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeCycle.OnTimeUpdatedEvent += OnTimeUpdated;

        private void OnDestroy() =>
            _timeCycle.OnTimeUpdatedEvent -= OnTimeUpdated;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateSunRotation(float timeOfDay)
        {
            Quaternion sunRotation = transform.rotation;
            float x = Mathf.Lerp(-90f, 270f, timeOfDay);
            float y = sunRotation.y;
            float z = sunRotation.z;

            transform.rotation = Quaternion.Euler(x, y, z);
        }

        private void UpdateLighting(float timeOfDay)
        {
            RenderSettings.ambientEquatorColor = _timeConfig.EquatorColor.Evaluate(timeOfDay);
            RenderSettings.ambientSkyColor = _timeConfig.SkyColor.Evaluate(timeOfDay);
            
            _light.color = _timeConfig.SunColor.Evaluate(timeOfDay);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime)
        {
            float timeOfDay = _timeCycle.GetTimeOfDay();

            UpdateSunRotation(timeOfDay);
            UpdateLighting(timeOfDay);
        }
    }
}