using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Observers.Gameplay.UI;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.GameTimer
{
    public class GameTimerUI : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ITimeCycle timeCycle, IUIObserver uiObserver)
        {
            _timeCycle = timeCycle;
            _uiObserver = uiObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _timeTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private ITimeCycle _timeCycle;
        private IUIObserver _uiObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _timeCycle.OnMinutePassedEvent += OnMinutePassed;

            _uiObserver.OnTriggerUIEvent += OnTriggerUIEvent;
        }

        private void Start() => UpdateTime();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _timeCycle.OnMinutePassedEvent -= OnMinutePassed;
            
            _uiObserver.OnTriggerUIEvent -= OnTriggerUIEvent;
        }

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
        
        private void OnMinutePassed() => UpdateTime();

        private void OnTriggerUIEvent(UIEventType eventType)
        {
            switch (eventType)
            {
                case UIEventType.ShowGameTimer:
                    Show();
                    break;
                
                case UIEventType.HideGameTimer:
                    Hide();
                    break;
            }
        }
    }
}