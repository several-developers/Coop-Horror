using GameCore.Enums.Gameplay;
using GameCore.Gameplay.GameManagement;
using GameCore.UI.Global;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.UI.Gameplay.HUD.GoldCounter
{
    public class GoldCounterView : UIElement
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameManagerDecorator gameManagerDecorator) =>
            _gameManagerDecorator = gameManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _goldTMP;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IGameManagerDecorator _gameManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _gameManagerDecorator.OnGameStateChangedEvent += OnGameStateChanged;
            _gameManagerDecorator.OnPlayersGoldChangedEvent += OnPlayersGoldChanged;
        }

        private void OnDestroy()
        {
            _gameManagerDecorator.OnGameStateChangedEvent -= OnGameStateChanged;
            _gameManagerDecorator.OnPlayersGoldChangedEvent -= OnPlayersGoldChanged;
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

        private void UpdateGoldText(int playersGold) =>
            _goldTMP.text = $"Gold: {playersGold}";

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGameStateChanged(GameState gameState) => HandleGameState(gameState);

        private void OnPlayersGoldChanged(int playersGold) => UpdateGoldText(playersGold);
    }
}