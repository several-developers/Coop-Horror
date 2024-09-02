using GameCore.Observers.Gameplay.Time;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Particles
{
    public class LevelTimedParticles : LevelParticlesBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeObserver timeObserver) =>
            _timeObserver = timeObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, MinMaxSlider(minValue: 0, maxValue: 1440, showFields: true)]
        [OnValueChanged(nameof(UpdateSpawnTimeText))]
        private Vector2Int _activePeriod = new(x: 0, y: 1440); // 1440 minutes in a day.

        [SerializeField, ReadOnly]
        [LabelText("Converted Time")]
        private string _activePeriodText;

        // FIELDS: --------------------------------------------------------------------------------

        private ITimeObserver _timeObserver;
        private bool _isActivePeriod;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _timeObserver.OnMinutePassedEvent += OnMinutePassed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _timeObserver.OnMinutePassedEvent -= OnMinutePassed;
        }

#if UNITY_EDITOR

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            UpdateSpawnTimeText();
        }
#endif
        
        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void EnableParticles()
        {
            if (!_isActivePeriod)
                return;

            base.EnableParticles();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckParticlesActivePeriod()
        {
            int currentTimeInMinutes = _timeObserver.GetCurrentTimeInMinutes();
            _isActivePeriod = currentTimeInMinutes >= _activePeriod.x && currentTimeInMinutes <= _activePeriod.y;
        }

        private void UpdateSpawnTimeText()
        {
            float minHourF = _activePeriod.x / 60f;
            int minHour = Mathf.FloorToInt(minHourF);
            int minMinute = _activePeriod.x - minHour * 60;

            float maxHourF = _activePeriod.y / 60f;
            int maxHour = Mathf.FloorToInt(maxHourF);
            int maxMinute = _activePeriod.y - maxHour * 60;

            _activePeriodText = $"{minHour:D2}:{minMinute:D2} - {maxHour:D2}:{maxMinute:D2}";
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMinutePassed() => CheckParticlesActivePeriod();
    }
}