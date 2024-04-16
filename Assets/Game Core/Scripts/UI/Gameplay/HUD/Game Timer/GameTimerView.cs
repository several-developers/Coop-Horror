using GameCore.Gameplay.GameTimeManagement;
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

        // FIELDS: --------------------------------------------------------------------------------

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
            MyDateTime dateTime = _timeCycle.GetDateTime();
            UpdateTime(dateTime.Minute, dateTime.Hour, dateTime.Day);
        }

        private void UpdateTime(int minute, int hour, int day)
        {
            string time = string.Format($"{hour:D2}:{minute:D2}");
            _timeTMP.text = $"{time}\nDay: {day}";
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHourPassed() => UpdateTime();
    }
}