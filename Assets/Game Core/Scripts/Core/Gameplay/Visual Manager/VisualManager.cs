using System.Collections.Generic;
using GameCore.Configs.Gameplay.Lighting;
using GameCore.Configs.Gameplay.Visual;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Locations;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.PubSub;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Infrastructure.Providers.Gameplay.LocationsMeta;
using GameCore.Observers.Gameplay.Time;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace GameCore.Gameplay.VisualManagement
{
    public class VisualManager : MonoBehaviour, IVisualManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IGameManagerDecorator gameManagerDecorator,
            PlayerCamera playerCamera,
            Sun sun,
            ITimeObserver timeObserver,
            IGameplayConfigsProvider gameplayConfigsProvider,
            ILocationsMetaProvider locationsMetaProvider,
            IPublisher<VisualPresetMessage> visualPresetPublisher,
            ISubscriber<VisualPresetMessage> visualPresetSubscriber
        )
        {
            _gameManagerDecorator = gameManagerDecorator;
            _timeObserver = timeObserver;
            _gameplayConfigsProvider = gameplayConfigsProvider;
            _locationsMetaProvider = locationsMetaProvider;
            _visualPresetPublisher = visualPresetPublisher;
            _visualPresetSubscriber = visualPresetSubscriber;
            _visualController = new VisualController(gameObject, playerCamera, sun, _volumeOne, _volumeTwo);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Volume _volumeOne;

        [SerializeField, Required]
        private Volume _volumeTwo;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<VisualPresetType, VisualPresetMeta> _visualPresets = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeObserver _timeObserver;
        private IGameplayConfigsProvider _gameplayConfigsProvider;
        private ILocationsMetaProvider _locationsMetaProvider;
        private IPublisher<VisualPresetMessage> _visualPresetPublisher;
        private ISubscriber<VisualPresetMessage> _visualPresetSubscriber;

        private VisualController _visualController;
        private VisualPresetType _previousPresetType;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            SetupPresetsDictionary();

            _visualPresetSubscriber.Subscribe(OnUpdateVisualPreset);

            _timeObserver.OnTimeUpdatedEvent += OnTimeUpdated;

            PlayerEntity.OnPlayerSpawnedEvent += OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent += OnPlayerDespawned;
        }

#warning ВЫНЕСТИ В GAME STATES HANDLER
        private void Start()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = gameState == GameState.Gameplay;

            if (!isGameStateValid)
                return;

            ChangePreset(VisualPresetType.Metro, instant: true);
        }

        private void OnDestroy()
        {
            _visualPresetSubscriber.Unsubscribe(OnUpdateVisualPreset);

            _timeObserver.OnTimeUpdatedEvent -= OnTimeUpdated;

            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [Button(ButtonStyle.FoldoutButton), DisableInEditorMode]
        public void ChangePreset(VisualPresetType presetType, bool instant = false)
        {
            UpdateLightingPreset(); // TEMP
            UpdateLightingSettings(); // TEMP

            bool isPresetTypeValid = presetType != _previousPresetType;

            if (!isPresetTypeValid)
                return;

            bool isPresetFound = TryGetPresetConfig(presetType, out VisualPresetMeta presetConfig);

            if (!isPresetFound)
            {
                Log.PrintError(log: $"Visual Preset Config with type <gb>({presetType})</gb> <rb>not found</rb>!");
                return;
            }

            string log = Log.HandleLog($"Changing visual preset to the <gb>{presetType.GetNiceName()}</gb>.");
            Debug.Log(log);

            _previousPresetType = presetType;

            _visualController.UpdateVisual(presetConfig, instant);
        }

        public void ChangePresetForAll(VisualPresetType presetType, bool instant = false)
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            var message = new VisualPresetMessage
            {
                PresetType = presetType,
                Instant = instant
            };

            _visualPresetPublisher.Publish(message);
        }

        [Button(ButtonStyle.FoldoutButton), DisableInEditorMode]
        public void SetLocationPreset(bool instant = false)
        {
#warning TEMP
            ChangePreset(VisualPresetType.ForestLocation, instant);
        }

        public void SetLocationPresetForAll(bool instant = false)
        {
#warning TEMP
            ChangePresetForAll(VisualPresetType.ForestLocation, instant);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPresetsDictionary()
        {
            var visualConfig = _gameplayConfigsProvider.GetConfig<VisualConfigMeta>();
            IEnumerable<VisualPresetMeta> allPresetsConfigs = visualConfig.GetAllPresetsConfigs();

            foreach (VisualPresetMeta presetConfig in allPresetsConfigs)
            {
                VisualPresetType presetType = presetConfig.PresetType;
                bool containsKey = _visualPresets.ContainsKey(presetType);

                if (containsKey)
                {
                    Log.PrintError(log: $"Dictionary <rb>already contains key</rb> {presetType}.");
                    continue;
                }

                _visualPresets.Add(presetType, presetConfig);
            }
        }

        private void UpdateLightingPreset()
        {
            LocationName currentLocation = _gameManagerDecorator.GetCurrentLocation();

            bool isLocationMetaFound =
                _locationsMetaProvider.TryGetLocationMeta(currentLocation, out LocationMeta locationMeta);

            if (!isLocationMetaFound)
                return;

            LightingPresetMeta lightingPreset = locationMeta.LightingPreset;
            _visualController.SetLightingPreset(lightingPreset);
        }

        private void UpdateLightingSettings()
        {
            float timeOfDay = _timeObserver.GetDateTimeNormalized();
            _visualController.UpdateLightingSettings(timeOfDay);
        }

        private bool TryGetPresetConfig(VisualPresetType presetType, out VisualPresetMeta preset) =>
            _visualPresets.TryGetValue(presetType, out preset);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnUpdateVisualPreset(VisualPresetMessage message) =>
            ChangePreset(message.PresetType, message.Instant);

        private void OnTimeUpdated(MyDateTime dateTime) => UpdateLightingSettings();

        private void OnPlayerSpawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            playerEntity.OnPlayerLocationChangedEvent += OnPlayerLocationChanged;
        }

        private void OnPlayerDespawned(PlayerEntity playerEntity)
        {
            bool isLocalPlayer = playerEntity.IsLocalPlayer();

            if (!isLocalPlayer)
                return;

            playerEntity.OnPlayerLocationChangedEvent -= OnPlayerLocationChanged;
        }

        private void OnPlayerLocationChanged(EntityLocation location)
        {
            bool changeAmbientSkyColor = location switch
            {
                EntityLocation.Dungeon => true,
                EntityLocation.Stairs => true,
                _ => false
            };

            _visualController.TogglePlayerInDungeonState(changeAmbientSkyColor);
        }
    }
}