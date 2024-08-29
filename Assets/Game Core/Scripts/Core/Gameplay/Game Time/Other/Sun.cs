using GameCore.Configs.Gameplay.Time;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
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

        private bool _changeAmbientSkyColor = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _timeCycle.OnTimeUpdatedEvent += OnTimeUpdated;

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
        }

        private void OnDestroy()
        {
            _timeCycle.OnTimeUpdatedEvent -= OnTimeUpdated;
            
            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
        }

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
            return;
            RenderSettings.ambientEquatorColor = _timeConfig.EquatorColor.Evaluate(timeOfDay);

            if (_changeAmbientSkyColor)
            {
                if (RenderSettings.ambientMode != AmbientMode.Skybox)
                    RenderSettings.ambientMode = AmbientMode.Skybox;
                
                RenderSettings.ambientSkyColor = _timeConfig.SkyColor.Evaluate(timeOfDay);
            }
            else
            {
                if (RenderSettings.ambientMode != AmbientMode.Flat)
                    RenderSettings.ambientMode = AmbientMode.Flat;
                
                RenderSettings.ambientSkyColor = _timeConfig.AmbientColor;
            }
            
            _light.color = _timeConfig.SunColor.Evaluate(timeOfDay);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime)
        {
            float timeOfDay = _timeCycle.GetTimeOfDay();

            UpdateSunRotation(timeOfDay);
            UpdateLighting(timeOfDay);
        }

        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            playerEntity.OnPlayerLocationChangedEvent += OnPlayerLocationChanged;
        }

        private void OnPlayerLocationChanged(EntityLocation location)
        {
            _changeAmbientSkyColor = location switch
            {
                EntityLocation.Dungeon => false,
                _ => true
            };
        }
    }
}