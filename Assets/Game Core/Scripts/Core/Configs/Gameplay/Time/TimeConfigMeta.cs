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

        [Title("Dungeons Visual Settings")]
        [SerializeField]
        private Color _ambientColor = Color.black;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float CycleDurationInMinutes => _cycleDurationInMinutes;
        public float CycleLengthModifier => _cycleLengthModifier;
        public int Second => _second;
        public int Minute => _minute;
        public int Hour => _hour;
        public bool Simulate => _simulate;
        public bool StopAtNight => _stopAtNight;
        public Gradient SkyColor => _skyColor;
        public Gradient EquatorColor => _equatorColor;
        public Gradient SunColor => _sunColor;
        public Color AmbientColor => _ambientColor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}