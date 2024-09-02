using System.Collections.Generic;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Visual;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Observers.Gameplay.Time;
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
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            TimeConfigMeta timeConfig = gameplayConfigsProvider.GetTimeConfig();

            _gameManagerDecorator = gameManagerDecorator;
            _timeObserver = timeObserver;
            _gameplayConfigsProvider = gameplayConfigsProvider;
            _visualController = new VisualController(gameObject, playerCamera, sun, _volumeOne, _volumeTwo, timeConfig);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Volume _volumeOne;

        [SerializeField, Required]
        private Volume _volumeTwo;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Dictionary<VisualPresetType, VisualPresetConfig> _visualPresets = new();

        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeObserver _timeObserver;
        private IGameplayConfigsProvider _gameplayConfigsProvider;

        private VisualController _visualController;
        private VisualPresetType _previousPresetType;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            SetupPresetsDictionary();

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
            _timeObserver.OnTimeUpdatedEvent -= OnTimeUpdated;

            PlayerEntity.OnPlayerSpawnedEvent -= OnPlayerSpawned;
            PlayerEntity.OnPlayerDespawnedEvent -= OnPlayerDespawned;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [Button(ButtonStyle.FoldoutButton), DisableInEditorMode]
        public void ChangePreset(VisualPresetType presetType, bool instant = false)
        {
            bool isPresetTypeValid = presetType != _previousPresetType;

            if (!isPresetTypeValid)
                return;

            bool isPresetFound = TryGetPresetConfig(presetType, out VisualPresetConfig presetConfig);

            if (!isPresetFound)
            {
                Log.PrintError(log: $"Visual Preset Config with type <gb>({presetType})</gb> <rb>not found</rb>!");
                return;
            }

            string log = Log.HandleLog($"Changing visual preset to the <gb>{presetType}</gb>.");
            Debug.Log(log);

            _previousPresetType = presetType;

            _visualController.ApplyEffects(presetConfig, instant);
        }

        [Button(ButtonStyle.FoldoutButton), DisableInEditorMode]
        public void SetLocationPreset(bool instant = false)
        {
            ChangePreset(VisualPresetType.ForestLocation, instant);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPresetsDictionary()
        {
            VisualConfigMeta visualConfig = _gameplayConfigsProvider.GetVisualConfig();
            IEnumerable<VisualPresetConfig> allPresetsConfigs = visualConfig.GetAllPresetsConfigs();

            foreach (VisualPresetConfig presetConfig in allPresetsConfigs)
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

        private bool TryGetPresetConfig(VisualPresetType presetType, out VisualPresetConfig presetConfig) =>
            _visualPresets.TryGetValue(presetType, out presetConfig);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnTimeUpdated(MyDateTime dateTime)
        {
            float timeOfDay = _timeObserver.GetDateTimeNormalized();
            
            _visualController.UpdateRenderSettings(timeOfDay);
            _visualController.UpdateLightSettings(timeOfDay);
        }

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
                EntityLocation.Dungeon => false,
                _ => true
            };

            _visualController.ToggleAmbientSkyColorState(changeAmbientSkyColor);
        }
    }
}