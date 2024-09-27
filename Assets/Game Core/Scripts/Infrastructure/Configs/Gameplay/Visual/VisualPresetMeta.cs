using CustomEditors;
using DG.Tweening;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameCore.Infrastructure.Configs.Gameplay.Visual
{
    public class VisualPresetMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(title: Constants.Settings)]
        [BoxGroup(MainSettingsTitle), SerializeField]
        private VisualPresetType _presetType;

        [BoxGroup(MainSettingsTitle), SerializeField, Min(0f)]
        private float _changeDuration = 0.5f;

        [BoxGroup(MainSettingsTitle), SerializeField]
        private Ease _changeEase = Ease.InQuad;

        [BoxGroup(VolumeProfileTitle), SerializeField]
        private bool _useVolumeProfile;
        
        [BoxGroup(VolumeProfileTitle), SerializeField, Required]
        [ShowIf(nameof(_useVolumeProfile))]
        private VolumeProfile _volumeProfile;
        
        [BoxGroup(NativeFogTitle), SerializeField]
        private bool _useNativeFog;
        
        [BoxGroup(NativeFogTitle), SerializeField]
        [ShowIf(nameof(_useNativeFog))]
        private Color _nativeFogColor = Color.black;

        [BoxGroup(NativeFogTitle), SerializeField, Min(0f)]
        [ShowIf(nameof(_useNativeFog))]
        private float _nativeFogDensity = 0.05f;
        
        [BoxGroup(CameraTitle), SerializeField]
        private bool _changeCameraDistance;

        [BoxGroup(CameraTitle), SerializeField, Min(0.1f)]
        [ShowIf(nameof(_changeCameraDistance))]
        private float _cameraDistance = 600f;

        [BoxGroup(CameraTitle), SerializeField]
        private bool _useSkybox = true;

        [BoxGroup(LightingTitle), SerializeField, Range(0f, 5f)]
        private float _sunIntensity = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public VisualPresetType PresetType => _presetType;
        public float ChangeDuration => _changeDuration;
        public Ease ChangeEase => _changeEase;
        
        public bool UseVolumeProfile => _useVolumeProfile;
        public VolumeProfile VolumeProfile => _volumeProfile;
        
        public bool UseNativeFog => _useNativeFog;
        public Color NativeFogColor => _nativeFogColor;
        public float NativeFogDensity => _nativeFogDensity;
        
        public bool ChangeCameraDistance => _changeCameraDistance;
        public float CameraDistance => _cameraDistance;
        public bool UseSkybox => _useSkybox;

        public float SunIntensity => _sunIntensity;

        // FIELDS: --------------------------------------------------------------------------------

        private const string MainSettingsTitle = Constants.Settings + "/Main Settings";
        private const string VolumeProfileTitle = "Volume Profile";
        private const string NativeFogTitle = "Native Fog";
        private const string CameraTitle = "Camera";
        private const string LightingTitle = "Lighting";
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.VisualPresetsCategory;
    }
}