using DG.Tweening;
using GameCore.Configs.Gameplay.Lighting;
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
            Volume volumeTwo
        )
        {
            _owner = owner;
            _playerCamera = playerCamera;
            _sun = sun;
            _volumeOne = volumeOne;
            _volumeTwo = volumeTwo;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly GameObject _owner;
        private readonly PlayerCamera _playerCamera;
        private readonly Sun _sun;
        private readonly Volume _volumeOne;
        private readonly Volume _volumeTwo;

        private Tweener _volumeTN;
        private Tweener _nativeFogTN;
        private Tweener _cameraTN;

        private LightingPresetMeta _lightingPreset;
        private VisualPresetMeta _visualPreset;
        private bool _playerInDungeon = true;
        private bool _isLightingPresetExists;
        private bool _isVisualPresetExists;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateVisual(VisualPresetMeta preset, bool instant = false)
        {
            _visualPreset = preset;
            _isVisualPresetExists = preset != null;
            
            ChangeVolume(preset, instant);
            ChangeNativeFog(preset, instant);
            ChangeCameraDistance(preset, instant);
            ChangeCameraBackgroundType(preset);
        }

        public void SetLightingPreset(LightingPresetMeta lightingPreset)
        {
            _lightingPreset = lightingPreset;
            _isLightingPresetExists = lightingPreset != null;
        }

        public void UpdateLightingSettings(float timeOfDay)
        {
            if (!_isLightingPresetExists)
                return;

            UpdateAmbientMode();
            UpdateAmbientColor();
            UpdateAmbientReflections();
            UpdateSunIntensity();
            UpdateNativeFogColor();
            UpdateNativeFogDensity();
            
            //float dotProduct = Vector3.Dot(_sun.transform.forward, Vector3.down);
            //float dotProductClamped = Mathf.Clamp01(dotProduct);

            // LOCAL METHODS: -----------------------------

            void UpdateAmbientMode()
            {
                AmbientMode ambientMode = GetAmbientMode();
                SetAmbientMode(ambientMode);
            }
            
            void UpdateAmbientColor()
            {
                Color ambientColor = GetAmbientColor();
                RenderSettings.ambientSkyColor = ambientColor;
            }

            void UpdateAmbientReflections()
            {
                if (_playerInDungeon)
                    return;
                
                float reflectionIntensity = _lightingPreset.AmbientReflectionsCurve.Evaluate(timeOfDay);
                RenderSettings.reflectionIntensity = reflectionIntensity;
            }

            void UpdateSunIntensity()
            {
                if (_playerInDungeon)
                    return;

                float sunIntensity = GetSunIntensity();
                _sun.SetIntensity(sunIntensity);
            }

            void UpdateNativeFogColor()
            {
                if (_playerInDungeon)
                    return;

                Color fogColor = GetNativeFogColor();
                RenderSettings.fogColor = fogColor;
            }

            void UpdateNativeFogDensity()
            {
                if (_playerInDungeon)
                    return;
                
                float fogDensity = GetNativeFogDensity();
                RenderSettings.fogDensity = fogDensity;
            }

            void SetAmbientMode(AmbientMode ambientMode)
            {
                if (RenderSettings.ambientMode == ambientMode)
                    return;

                RenderSettings.ambientMode = ambientMode;
            }

            AmbientMode GetAmbientMode()
            {
                bool useFlat = _playerInDungeon || _isVisualPresetExists && !_visualPreset.UseSkybox;
                return useFlat ? AmbientMode.Flat : AmbientMode.Skybox;
            }
            
            Color GetAmbientColor()
            {
                if (_playerInDungeon)
                    return Color.black;

                Color dayAmbient = _lightingPreset.DayAmbient;
                Color nightAmbient = _lightingPreset.NightAmbient;
                float t = _lightingPreset.AmbientColorCurve.Evaluate(timeOfDay);
                Color ambientColor = Color.Lerp(a: nightAmbient, b: dayAmbient, t);
                return ambientColor;
            }

            Color GetNativeFogColor()
            {
                bool overrideNativeFogColor = _lightingPreset.OverrideNativeFogColor;
                bool useVisualPresetFogColor = !overrideNativeFogColor && _isVisualPresetExists;

                if (useVisualPresetFogColor)
                    return _visualPreset.NativeFogColor;

                Color dayFog = _lightingPreset.DayFog;
                Color nightFog = _lightingPreset.NightFog;
                float t = _lightingPreset.FogColorCurve.Evaluate(timeOfDay);
                Color fogColor = Color.Lerp(a: nightFog, b: dayFog, t);
                return fogColor;
            }

            float GetNativeFogDensity()
            {
                bool overrideNativeFogDensity = _lightingPreset.OverrideNativeFogDensity;
                bool useVisualPresetFogDensity = !overrideNativeFogDensity && _isVisualPresetExists;

                float fogDensity = useVisualPresetFogDensity
                    ? _visualPreset.NativeFogDensity
                    : _lightingPreset.NativeFogDensity;

                return fogDensity;
            }
            
            float GetSunIntensity()
            {
                bool overrideSunIntensity = _lightingPreset.OverrideSunIntensity && _isVisualPresetExists;
                
                float sunIntensity = overrideSunIntensity
                    ? _lightingPreset.SunIntensity
                    : _visualPreset.SunIntensity;
                
                float t = _lightingPreset.SunIntensityCurve.Evaluate(timeOfDay);
                float intensity = Mathf.Lerp(a: 0f, b: sunIntensity, t);
                return intensity;
            }
        }

        public void TogglePlayerInDungeonState(bool playerInDungeon) =>
            _playerInDungeon = playerInDungeon;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ChangeVolume(VisualPresetMeta preset, bool instant = false)
        {
            float duration = instant ? 0f : preset.ChangeDuration;
            Ease ease = preset.ChangeEase;

            _volumeTN.Kill();

            _volumeTwo.profile = preset.UseVolumeProfile ? preset.VolumeProfile : null;
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
                    _volumeOne.profile = preset.UseVolumeProfile ? preset.VolumeProfile : null;
                    _volumeTwo.profile = null;
                    _volumeOne.weight = 1f;
                    _volumeTwo.weight = 0f;
                });
        }

        private void ChangeNativeFog(VisualPresetMeta preset, bool instant = false)
        {
            bool ignoreColorChange = false;
            bool ignoreDensityChange = false;
            
            if (_isLightingPresetExists)
            {
                ignoreColorChange = _lightingPreset.OverrideNativeFogColor;
                ignoreDensityChange = _lightingPreset.OverrideNativeFogDensity;
            }
            
            bool enableFog = preset.UseNativeFog;
            float densityFrom = RenderSettings.fogDensity;
            float densityTo = enableFog ? preset.NativeFogDensity : 0f;
            float duration = instant ? 0f : preset.ChangeDuration;
            Color colorFrom = RenderSettings.fogColor;
            Color colorTo = preset.NativeFogColor;
            Ease ease = preset.ChangeEase;

            if (enableFog && !RenderSettings.fog)
                RenderSettings.fog = true;

            _nativeFogTN.Kill();

            _nativeFogTN = DOVirtual
                .Float(from: 0f, to: 1f, duration, onVirtualUpdate: t =>
                {
                    Color color = Color.Lerp(a: colorFrom, b: colorTo, t);
                    float density = Mathf.Lerp(a: densityFrom, b: densityTo, t);

                    if (!ignoreColorChange)
                        RenderSettings.fogColor = color;

                    if (!ignoreDensityChange)
                        RenderSettings.fogDensity = density;
                })
                .SetEase(ease)
                .SetLink(_owner)
                .OnComplete(() => { RenderSettings.fog = enableFog; });
        }

        private void ChangeCameraDistance(VisualPresetMeta preset, bool instant = false)
        {
            Camera mainCamera = _playerCamera.CameraReferences.MainCamera;
            float distanceFrom = mainCamera.farClipPlane;
            float distanceTo = preset.ChangeCameraDistance ? preset.CameraDistance : distanceFrom;
            float duration = instant ? 0f : preset.ChangeDuration;
            Ease ease = preset.ChangeEase;

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

        private void ChangeCameraBackgroundType(VisualPresetMeta preset)
        {
            bool useSkybox = preset.UseSkybox;
            Camera mainCamera = _playerCamera.CameraReferences.MainCamera;
            mainCamera.clearFlags = useSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        }
    }
}