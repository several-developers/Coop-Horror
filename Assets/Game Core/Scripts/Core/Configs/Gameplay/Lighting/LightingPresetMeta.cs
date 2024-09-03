using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Lighting
{
    public class LightingPresetMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(AmbientTitle)]
        [BoxGroup(AmbientGroup, showLabel: false), SerializeField]
        private AnimationCurve _ambientColorCurve;
        
        [BoxGroup(AmbientGroup), SerializeField]
        private AnimationCurve _ambientReflectionsCurve;
        
        [BoxGroup(AmbientGroup), SerializeField, ColorUsage(showAlpha: true, hdr: true)]
        private Color _dayAmbient;
        
        [BoxGroup(AmbientGroup), SerializeField, ColorUsage(showAlpha: true, hdr: true)]
        private Color _nightAmbient;
        
        [TitleGroup(SunTitle)]
        [BoxGroup(SunGroup, showLabel: false), SerializeField]
        private AnimationCurve _sunIntensityCurve;

        [BoxGroup(SunGroup), SerializeField]
        private bool _overrideSunIntensity;
        
        [BoxGroup(SunGroup), SerializeField]
        [ShowIf(nameof(_overrideSunIntensity))]
        private float _sunIntensity = 1.5f;

        [TitleGroup(FogTitle)]
        [BoxGroup(FogGroup, showLabel: false), SerializeField]
        private bool _overrideNativeFogColor;
        
        [BoxGroup(FogGroup), SerializeField]
        [ShowIf(nameof(_overrideNativeFogColor))]
        private AnimationCurve _fogColorCurve;

        [BoxGroup(FogGroup), SerializeField]
        [ShowIf(nameof(_overrideNativeFogColor))]
        private Color _dayFog;
        
        [BoxGroup(FogGroup), SerializeField]
        [ShowIf(nameof(_overrideNativeFogColor))]
        private Color _nightFog;

        [BoxGroup(FogGroup), SerializeField]
        private bool _overrideNativeFogDensity;

        [BoxGroup(FogGroup), SerializeField, Min(0f)]
        [ShowIf(nameof(_overrideNativeFogDensity))]
        private float _nativeFogDensity = 0.05f;

        // ReSharper disable once NotAccessedField.Local
        [SerializeField, ColorUsage(showAlpha: true, hdr: true), Space(height: 15), ReadOnly]
        private Color _defaultAmbient = new(r: 0.212f, g: 0.227f, b: 0.259f);

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public AnimationCurve AmbientColorCurve => _ambientColorCurve;
        public AnimationCurve AmbientReflectionsCurve => _ambientReflectionsCurve;
        public Color DayAmbient => _dayAmbient;
        public Color NightAmbient => _nightAmbient;
        
        public AnimationCurve SunIntensityCurve => _sunIntensityCurve;
        public bool OverrideSunIntensity => _overrideSunIntensity;
        public float SunIntensity => _sunIntensity;
        
        public bool OverrideNativeFogColor => _overrideNativeFogColor;
        public AnimationCurve FogColorCurve => _fogColorCurve;
        public Color DayFog => _dayFog;
        public Color NightFog => _nightFog;
        public bool OverrideNativeFogDensity => _overrideNativeFogDensity;
        public float NativeFogDensity => _nativeFogDensity;
        
        // FIELDS: --------------------------------------------------------------------------------

        private const string AmbientTitle = "Ambient";
        private const string SunTitle = "Sun";
        private const string FogTitle = "Native Fog";
        
        private const string AmbientGroup = AmbientTitle + "/In";
        private const string SunGroup = SunTitle + "/In";
        private const string FogGroup = FogTitle + "/In";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}