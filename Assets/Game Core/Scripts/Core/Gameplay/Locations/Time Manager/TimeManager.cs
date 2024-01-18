using System;
using GameCore.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Locations
{
    public class TimeManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private float _cycleDurationInMinutes = 5f;
        
        [SerializeField, Range(0.1f, 10f)]
        private float _cycleLengthModifier = 1f;
        
        [SerializeField, Range(0, 60)]
        private int _second;
        
        [SerializeField, Range(0, 60)]
        private int _minute;
        
        [SerializeField, Range(0, 23)]
        private int _hour = 12;

        [SerializeField]
        private bool _simulate;

        [SerializeField]
        private bool _stopAtNight;

        [SerializeField]
        private AnimationCurve _sunIntensityCurve;
        
        [SerializeField]
        private Gradient _skyColor;
        
        [SerializeField]
        private Gradient _equatorColor;
        
        [SerializeField]
        private Gradient _sunColor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Light _sun;

        // PROPERTIES: ----------------------------------------------------------------------------

        private int Second
        {
            get => _second;
            set => SetDateTime(second: value, _minute, _hour);
        }
        
        private int Minute
        {
            get => _minute;
            set => SetDateTime(_second, minute: value, _hour);
        }

        private int Hour
        {
            get => _hour;
            set => SetDateTime(_second, _minute, hour: value);
        }
        
        // FIELDS: --------------------------------------------------------------------------------

        private Transform _sunTransform;
        private DayTime _dayTime;
        private DateTime _date;
        private float _internalTimeOverflow;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _sunTransform = _sun.transform;

        private void Start()
        {
            SetDateTime(_second, _minute, _hour);
            UpdateDayTime();
        }

        private void Update() => UpdateModule();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateModule()
        {
            if (!_simulate)
                return;
            
            float t = 24f / 60f / (_cycleDurationInMinutes * _cycleLengthModifier);
            t = t * 3600f * Time.deltaTime;

            if (t < 1f)
                _internalTimeOverflow += t;
            else
                _internalTimeOverflow = t;

            Second += (int)_internalTimeOverflow;

            if (_internalTimeOverflow >= 1f)
                _internalTimeOverflow = 0f;

            SetDateTime(_second, _minute, _hour);

            float timeOfDay = GetTimeOfDay();
            
            UpdateDayTime();
            UpdateLighting(timeOfDay);
            UpdateSunRotation(timeOfDay);
        }
        
        private void UpdateDayTime()
        {
            switch (_hour)
            {
                case >= 6 and <= 8: _dayTime = DayTime.Sunrise; return;
                case > 8 and < 18: _dayTime = DayTime.Day; return;
                case >= 18 and < 22: _dayTime = DayTime.Sunset; return;
                case >= 22 or < 6: _dayTime = DayTime.Night; break;
            }
        }

        private void UpdateLighting(float timeOfDay)
        {
            RenderSettings.ambientEquatorColor = _equatorColor.Evaluate(timeOfDay);
            RenderSettings.ambientSkyColor = _skyColor.Evaluate(timeOfDay);
                
            _sun.color = _sunColor.Evaluate(timeOfDay);
            //_sun.intensity = _sunIntensityCurve.Evaluate(timeOfDay);
        }

        private void UpdateSunRotation(float timeOfDay)
        {
            Quaternion sunRotation = _sunTransform.rotation;
            float x = Mathf.Lerp(-90f, 270f, timeOfDay);
            float y = sunRotation.y;
            float z = sunRotation.z;
            
            _sunTransform.rotation = Quaternion.Euler(x, y, z);
        }

        private void SetDateTime(int second, int minute, int hour)
        {
            _second = second;
            _minute = minute;
            _hour = hour;

            DateTime currentTime = new();
            currentTime = currentTime.AddHours(hour);
            currentTime = currentTime.AddMinutes(minute);
            currentTime = currentTime.AddSeconds(second);

            if (_date.Hour != currentTime.Hour)
            {
                // Hour passed
            }

            _date = currentTime;
            _second = _date.Second;
            _minute = _date.Minute;
            _hour = _date.Hour;
        }

        private float GetTimeOfDay()
        {
            const int secondsInDay = 86400;
            const int secondsInHour = 3600;
            const int secondsInMinute = 60;

            float totalSeconds = _hour * secondsInHour +
                                 _minute * secondsInMinute +
                                 _second;
            
            float timeOfDay = Mathf.Clamp01(totalSeconds / (float)secondsInDay);
            return timeOfDay;
        }
    }
}