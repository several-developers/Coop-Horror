using System.Collections.Generic;
using DG.Tweening;
using GameCore.Configs.Gameplay.Visual;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
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
        private void Construct(IGameManagerDecorator gameManagerDecorator, PlayerCamera playerCamera,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _playerCamera = playerCamera;
            
            SetupPresetsDictionary(gameplayConfigsProvider);
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
        private PlayerCamera _playerCamera;
        private Tweener _volumeTN;
        private Tweener _nativeFogTN;
        private Tweener _cameraTN;
        private VisualPresetType _previousPresetType;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

        private void Start()
        {
            GameState gameState = _gameManagerDecorator.GetGameState();
            bool isGameStateValid = gameState == GameState.ReadyToLeaveTheRoad;
            
            if (!isGameStateValid)
                return;
            
            ChangePreset(VisualPresetType.RoadLocation, instant: true);
        }

        private void OnDestroy() =>
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        [Button(ButtonStyle.FoldoutButton)]
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

            string log = Log.HandleLog($"Changing visual preset to the <gb>{presetType.GetNiceName()}</gb>.");
            Debug.Log(log);

            _previousPresetType = presetType;
            
            ApplyEffects(presetConfig, instant);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupPresetsDictionary(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            VisualConfigMeta visualConfig = gameplayConfigsProvider.GetVisualConfig();
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

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.ArrivedAtTheRoad:
                    ChangePreset(VisualPresetType.RoadLocation);
                    break;
                
                case GameState.HeadingToTheLocation:
                    ChangePreset(VisualPresetType.DefaultLocation);
                    break;
            }
        }

        private void ApplyEffects(VisualPresetConfig presetConfig, bool instant = false)
        {
            ChangeVolume(presetConfig, instant);
            ChangeNativeFog(presetConfig, instant);
            ChangeCameraDistance(presetConfig, instant);
            ChangeCameraBackgroundType(presetConfig);
        }

        private void ChangeVolume(VisualPresetConfig presetConfig, bool instant = false)
        {
            float duration = instant ? 0f : presetConfig.ChangeDuration;
            Ease ease = presetConfig.ChangeEase;

            _volumeTN.Kill();
            
            _volumeTwo.profile = presetConfig.UseVolumeProfile ? presetConfig.VolumeProfile : null;
            _volumeOne.weight = 1f;
            _volumeTwo.weight = 0f;
            
            _volumeTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float volumeOne = 1f - t;
                    float volumeTwo = t;

                    _volumeOne.weight = volumeOne;
                    _volumeTwo.weight = volumeTwo;
                })
                .SetEase(ease)
                .OnComplete(() =>
                {
                    _volumeOne.profile = presetConfig.UseVolumeProfile ? presetConfig.VolumeProfile : null;
                    _volumeTwo.profile = null;
                    _volumeOne.weight = 1f;
                    _volumeTwo.weight = 0f;
                });
        }

        private void ChangeNativeFog(VisualPresetConfig presetConfig, bool instant = false)
        {
            bool useNativeFog = presetConfig.UseNativeFog;
            float densityFrom = RenderSettings.fogDensity;
            float densityTo = useNativeFog ? presetConfig.NativeFogDensity : 0f;
            float duration = instant ? 0f : presetConfig.ChangeDuration;
            Color colorFrom = RenderSettings.fogColor;
            Color colorTo = presetConfig.NativeFogColor;
            Ease ease = presetConfig.ChangeEase;

            if (useNativeFog && !RenderSettings.fog)
                RenderSettings.fog = true;
            
            _nativeFogTN.Kill();
            
            _nativeFogTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float density = Mathf.Lerp(a: densityFrom, b: densityTo, t);
                    Color color = Color.Lerp(a: colorFrom, b: colorTo, t);

                    RenderSettings.fogDensity = density;
                    RenderSettings.fogColor = color;
                })
                .SetEase(ease)
                .OnComplete(() =>
                {
                    RenderSettings.fog = useNativeFog;
                });
        }

        private void ChangeCameraDistance(VisualPresetConfig presetConfig, bool instant = false)
        {
            Camera mainCamera = _playerCamera.CameraReferences.MainCamera;
            float distanceFrom = mainCamera.farClipPlane;
            float distanceTo = presetConfig.ChangeCameraDistance ? presetConfig.CameraDistance : distanceFrom;
            float duration = instant ? 0f : presetConfig.ChangeDuration;
            Ease ease = presetConfig.ChangeEase;

            _cameraTN.Kill();
            
            _cameraTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float distance = Mathf.Lerp(a: distanceFrom, b: distanceTo, t);
                    mainCamera.farClipPlane = distance;
                })
                .SetEase(ease);
        }

        private void ChangeCameraBackgroundType(VisualPresetConfig presetConfig)
        {
            bool useSkybox = presetConfig.UseSkybox;
            Camera mainCamera = _playerCamera.CameraReferences.MainCamera;
            mainCamera.clearFlags = useSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        }

        private bool TryGetPresetConfig(VisualPresetType presetType, out VisualPresetConfig presetConfig) =>
            _visualPresets.TryGetValue(presetType, out presetConfig);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
    }
}