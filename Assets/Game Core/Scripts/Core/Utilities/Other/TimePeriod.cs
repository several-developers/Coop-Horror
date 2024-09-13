using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Utilities
{
    [Serializable]
    public class TimePeriod
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public TimePeriod() => UpdateTimeText();

        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField]
        [MinMaxSlider(minValue: 0, maxValue: 1440, showFields: true)]
        [OnValueChanged(nameof(UpdateTimeText))]
        private Vector2Int _time = new(x: 0, y: Constants.MinutesInDay);

        [SerializeField, ReadOnly]
        [LabelText("Converted Time")]
        private string _timeText;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void UpdateTimeText()
        {
            float minHourF = _time.x / (float)Constants.MinutesInHour;
            int minHour = Mathf.FloorToInt(minHourF);
            int minMinute = _time.x - minHour * 60;
            
            float maxHourF = _time.y / 60f;
            int maxHour = Mathf.FloorToInt(maxHourF);
            int maxMinute = _time.y - maxHour * 60;

            _timeText = $"{minHour:D2}:{minMinute:D2} - {maxHour:D2}:{maxMinute:D2}";
        }

        public bool IsTimeValid(int currentTimeInMinutes) =>
            currentTimeInMinutes >= _time.x && currentTimeInMinutes <= _time.y;
    }
}