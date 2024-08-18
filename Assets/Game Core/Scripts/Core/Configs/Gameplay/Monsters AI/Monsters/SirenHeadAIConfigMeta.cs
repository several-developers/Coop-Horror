using System;
using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using Sonity;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Enemies
{
    public class SirenHeadAIConfigMeta : MonsterAIConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(CommonSettingsTitle)]
        [BoxGroup(CommonGroup, showLabel: false), SerializeField, Range(0.01f, 1f)]
        private float _animationSpeed = 1f;

        [BoxGroup(CommonGroup), SerializeField, Range(0, 1440)]
        [OnValueChanged(nameof(UpdateArriveTimeOffset))]
        [Tooltip("Отклонение по времени прибытия.")]
        private int _arriveTimeOffset;
        
        [BoxGroup(CommonGroup), SerializeField, ReadOnly]
        [LabelText("Converted Time")]
        private string _arriveTimeOffsetConverted;

        [TitleGroup(SoundSettingsTitle)]
        [BoxGroup(SoundGroup, showLabel: false), SerializeField]
        private SoundSettings _soundSettings;
        
        [Title(SFXTitle)]
        [SerializeField, Required]
        private SoundEvent _footstepsSE;
        
        // PROPERTIES: ----------------------------------------------------------------------------

        public float AnimationSpeed => _animationSpeed;
        public int ArriveTimeOffset => _arriveTimeOffset;

        public SoundEvent FootstepsSE => _footstepsSE;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private const string CommonSettingsTitle = "Common Settings";
        private const string SoundSettingsTitle = "Sound Settings";
        private const string SFXTitle = "SFX";

        private const string CommonGroup = CommonSettingsTitle + "/Group";
        private const string SoundGroup = SoundSettingsTitle + "/Group";

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateArriveTimeOffset();
            _soundSettings.Update();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public SoundSettings GetSoundSettings() => _soundSettings;
        
        public override MonsterType GetMonsterType() =>
            MonsterType.SirenHead;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void UpdateArriveTimeOffset()
        {
            float hourF = _arriveTimeOffset / 60f;
            int hour = Mathf.FloorToInt(hourF);
            int minute = _arriveTimeOffset - hour * 60;
            _arriveTimeOffsetConverted = $"{hour:D2}:{minute:D2}";
        }

        // INNER CLASSES: -------------------------------------------------------------------------

        #region Inner Classses

        [Serializable]
        public class SoundSettings
        {
            // MEMBERS: -------------------------------------------------------------------------------

            [SerializeField]
            private List<RoarConfig> _roarConfigs;

            // PUBLIC METHODS: ------------------------------------------------------------------------

            public void Update()
            {
                foreach (RoarConfig roarConfig in _roarConfigs)
                    roarConfig.UpdateTime();
            }
            
            public IReadOnlyList<RoarConfig> GetAllRoarConfigs() => _roarConfigs;

            // INNER CLASSES: -------------------------------------------------------------------------

            [Serializable]
            public class RoarConfig
            {
                // MEMBERS: -------------------------------------------------------------------------------

                [SerializeField, Range(0, 1440)]
                [OnValueChanged(nameof(UpdateTime))]
                private int _time;

                [SerializeField, ReadOnly]
                [LabelText("Converted Time")]
                private string _timeConverted;
                
                [SerializeField, Required]
                private SoundEvent _roarSE;

                // PROPERTIES: ----------------------------------------------------------------------------

                public int Time => _time;
                public SoundEvent RoarSE => _roarSE;

                // PUBLIC METHODS: ------------------------------------------------------------------------

                public void UpdateTime()
                {
                    float hourF = _time / 60f;
                    int hour = Mathf.FloorToInt(hourF);
                    int minute = _time - hour * 60;
                    _timeConverted = $"{hour:D2}:{minute:D2}";
                }
            }
        }

        #endregion
    }
}