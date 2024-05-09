using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
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
        private void Construct(IGameManagerDecorator gameManagerDecorator, ITimeCycle timeCycle)
        {
            _gameManagerDecorator = gameManagerDecorator;
            _timeCycle = timeCycle;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _timeTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameManagerDecorator _gameManagerDecorator;
        private ITimeCycle _timeCycle;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;

            _timeCycle.OnHourPassedEvent += OnHourPassed;
        }

        private void Start()
        {
            UpdateTime();
            Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
            
            _timeCycle.OnHourPassedEvent -= OnHourPassed;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleGameState(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.KillPlayersOnTheRoad:
                    Hide();
                    break;
                
                case GameState.RestartGame:
                    Show();
                    break;
            }
        }

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

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);
        
        private void OnHourPassed() => UpdateTime();
    }
}