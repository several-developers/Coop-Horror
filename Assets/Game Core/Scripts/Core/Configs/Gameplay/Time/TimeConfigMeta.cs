using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Time
{
    public class TimeConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.Settings)]
        [SerializeField]
        private float _cycleDurationInMinutes = 20f;

        [SerializeField, Range(0.1f, 10f)]
        private float _cycleLengthModifier = 1f;

        [SerializeField, Range(0, 60)]
        private int _second;

        [SerializeField, Range(0, 60)]
        private int _minute;

        [SerializeField, Range(0, 23)]
        private int _hour;

        [SerializeField]
        private bool _simulate;

        [SerializeField]
        private bool _stopAtNight;

        [Title("Location Visual Settings")]
        [SerializeField]
        private AnimationCurve _sunIntensityCurve;

        [SerializeField]
        private Gradient _skyColor;

        [SerializeField]
        private Gradient _equatorColor;

        [SerializeField]
        private Gradient _sunColor;

        [Space(10)]
        [SerializeField]
        public AnimationCurve _lightIntensityCurve;

        [SerializeField]
        public float _maxSunIntensity = 1.5f;

        [SerializeField]
        public float _maxMoonIntensity = 0.5f;

        [SerializeField, ColorUsage(showAlpha: true, hdr: true)]
        private Color _defaultAmbient;
        
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color _dayAmbient;
        
        [ColorUsage(showAlpha: true, hdr: true)]
        public Color _nightAmbient;
        
        [Space(5)]
        [SerializeField]
        public AnimationCurve _ambientReflectionsCurve;

        [Space(5)]
        [SerializeField]
        public AnimationCurve _fogColorCurve;

        [SerializeField]
        public Color _dayFog;
        
        [SerializeField]
        public Color _nightFog;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float CycleDurationInMinutes => _cycleDurationInMinutes;
        public float CycleLengthModifier => _cycleLengthModifier;
        public int Second => _second;
        public int Minute => _minute;
        public int Hour => _hour;
        public bool Simulate => _simulate;
        public bool StopAtNight => _stopAtNight;
        public AnimationCurve SunIntensityCurve => _sunIntensityCurve;
        public Gradient SkyColor => _skyColor;
        public Gradient EquatorColor => _equatorColor;
        public Gradient SunColor => _sunColor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}