using DG.Tweening;
using GameCore.Configs.Gameplay.Time;
using GameCore.Configs.Gameplay.Visual;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using GameCore.Gameplay.GameTimeManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameCore.Gameplay.VisualManagement
{
    public class VisualController
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public VisualController(
            GameObject owner,
            PlayerCamera playerCamera,
            Sun sun,
            Volume volumeOne,
            Volume volumeTwo,
            TimeConfigMeta timeConfig
        )
        {
            _owner = owner;
            _playerCamera = playerCamera;
            _sun = sun;
            _volumeOne = volumeOne;
            _volumeTwo = volumeTwo;
            _timeConfig = timeConfig;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _owner;
        private readonly PlayerCamera _playerCamera;
        private readonly Sun _sun;
        private readonly Volume _volumeOne;
        private readonly Volume _volumeTwo;
        private readonly TimeConfigMeta _timeConfig;

        private Tweener _volumeTN;
        private Tweener _nativeFogTN;
        private Tweener _cameraTN;
        private Tweener _sunTN;
        private Tweener _skyboxTN;

        private float _previousSunIntensity = 1.5f;
        private bool _changeAmbientSkyColor = true;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ApplyEffects(VisualPresetConfig presetConfig, bool instant = false)
        {
            ChangeVolume(presetConfig, instant);
            ChangeNativeFog(presetConfig, instant);
            ChangeCameraDistance(presetConfig, instant);
            ChangeCameraBackgroundType(presetConfig);
            ChangeSkyboxIntensity(presetConfig, instant);
            ChangeSun(presetConfig, instant);
        }
        
        public void UpdateRenderSettings(float timeOfDay)
        {
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

                RenderSettings.ambientSkyColor = Color.black;
            }
        }

        public void UpdateLightSettings(float timeOfDay)
        {
            float t = _timeConfig._lightIntensityCurve.Evaluate(timeOfDay);
            float sunIntensity = Mathf.Lerp(0, _timeConfig._maxSunIntensity, t);
            float moonIntensity = Mathf.Lerp(_timeConfig._maxMoonIntensity, 0, t);
            
            _sun.SetIntensity(sunIntensity);
        }

        public void ToggleAmbientSkyColorState(bool changeAmbientSkyColor) =>
            _changeAmbientSkyColor = changeAmbientSkyColor;

        // PRIVATE METHODS: -----------------------------------------------------------------------

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
                .SetLink(_owner)
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
                .SetLink(_owner)
                .OnComplete(() => { RenderSettings.fog = useNativeFog; });
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
                .SetEase(ease)
                .SetLink(_owner);
        }

        private void ChangeCameraBackgroundType(VisualPresetConfig presetConfig)
        {
            bool useSkybox = presetConfig.UseSkybox;
            Camera mainCamera = _playerCamera.CameraReferences.MainCamera;
            mainCamera.clearFlags = useSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        }

        private void ChangeSkyboxIntensity(VisualPresetConfig presetConfig, bool instant = false)
        {
            float intensityFrom = RenderSettings.ambientIntensity;
            float intensityTo = presetConfig.SkyboxIntensity;
            float duration = instant ? 0f : presetConfig.ChangeDuration;
            Ease ease = presetConfig.ChangeEase;

            _skyboxTN.Kill();

            _skyboxTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float intensity = Mathf.Lerp(a: intensityFrom, b: intensityTo, t);
                    RenderSettings.ambientIntensity = intensity;
                })
                .SetEase(ease)
                .SetLink(_owner);
        }

        private void ChangeSun(VisualPresetConfig presetConfig, bool instant = false)
        {
            _previousSunIntensity = presetConfig.SunIntensity;
            
            return;
            float intensityFrom = _sun.GetIntensity();
            float intensityTo = presetConfig.SunIntensity;
            float duration = instant ? 0f : presetConfig.ChangeDuration;
            Ease ease = presetConfig.ChangeEase;
            
            _sunTN.Kill();

            _sunTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    float intensity = Mathf.Lerp(a: intensityFrom, b: intensityTo, t);
                    _sun.SetIntensity(intensity);
                })
                .SetEase(ease)
                .SetLink(_owner);
        }
    }
}