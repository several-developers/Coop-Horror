using GameCore.InfrastructureTools.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.Time
{
    public class TimeConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [TitleGroup(title: Constants.Settings)]
        [BoxGroup(Constants.SettingsIn, showLabel: false), SerializeField]
        private float _cycleDurationInMinutes = 20f;

        [BoxGroup(Constants.SettingsIn), SerializeField, Range(0.1f, 10f)]
        private float _cycleLengthModifier = 1f;

        [BoxGroup(Constants.SettingsIn), SerializeField, Range(0, 60)]
        private int _startAtSecond;

        [BoxGroup(Constants.SettingsIn), SerializeField, Range(0, 60)]
        private int _startAtMinute;

        [BoxGroup(Constants.SettingsIn), SerializeField, Range(0, 23)]
        private int _startAtHour;

        [BoxGroup(Constants.SettingsIn), SerializeField]
        private bool _stopAtNight;

        [BoxGroup(Constants.SettingsIn), SerializeField]
        private bool _simulate;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float CycleDurationInMinutes => _cycleDurationInMinutes;
        public float CycleLengthModifier => _cycleLengthModifier;
        public int StartAtSecond => _startAtSecond;
        public int StartAtMinute => _startAtMinute;
        public int StartAtHour => _startAtHour;
        public bool StopAtNight => _stopAtNight;
        public bool Simulate => _simulate;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}