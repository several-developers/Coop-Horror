using System;
using GameCore.Gameplay.Locations.GameTime;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.GameTimer
{
    public class GameTimerView : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle) =>
            _timeCycle = timeCycle;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _timeTMP;

        private ITimeCycle _timeCycle;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _timeCycle.OnHourPassedEvent += OnHourPassed;

        private void Start()
        {
            UpdateTime();
            Show();
        }

        private void OnDestroy() =>
            _timeCycle.OnHourPassedEvent -= OnHourPassed;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateTime()
        {
            DateTime dateTime = _timeCycle.GetDateTime();
            UpdateTime(dateTime.Minute, dateTime.Hour);
        }

        private void UpdateTime(int minute, int hour)
        {
            string time = string.Format($"{hour:D2}:{minute:D2}");
            _timeTMP.text = time;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHourPassed() => UpdateTime();
    }
}