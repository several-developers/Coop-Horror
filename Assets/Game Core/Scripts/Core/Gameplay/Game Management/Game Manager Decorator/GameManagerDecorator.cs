using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManagerDecorator : IGameManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<GameState> OnGameStateChangedEvent = delegate { };
        public event Action<SceneName> OnSelectedLocationChangedEvent = delegate { };
        public event Action<int> OnPlayersGoldChangedEvent = delegate { };

        public event Action<GameState> OnChangeGameStateInnerEvent = delegate { };
        public event Action<GameState, GameState> OnChangeGameStateWhenAllPlayersReadyInnerEvent = delegate { };
        public event Action<SceneName> OnSelectLocationInnerEvent = delegate { };
        public event Action OnLoadSelectedLocationInnerEvent = delegate { };
        public event Action<int> OnAddPlayersGoldInnerEvent = delegate { };
        public event Action<int> OnSpendPlayersGoldInnerEvent = delegate { };
        public event Action OnResetPlayersGoldInnerEvent = delegate { };
        public event Func<SceneName> OnGetSelectedLocationInnerEvent;
        public event Func<GameState> OnGetGameStateInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void GameStateChanged(GameState gameState) =>
            OnGameStateChangedEvent.Invoke(gameState);

        public void SelectedLocationChanged(SceneName locationName) =>
            OnSelectedLocationChangedEvent.Invoke(locationName);

        public void PlayersGoldChanged(int playersGold) =>
            OnPlayersGoldChangedEvent.Invoke(playersGold);

        public void ChangeGameState(GameState gameState) =>
            OnChangeGameStateInnerEvent.Invoke(gameState);

        public void ChangeGameStateWhenAllPlayersReady(GameState newState, GameState previousState) =>
            OnChangeGameStateWhenAllPlayersReadyInnerEvent.Invoke(newState, previousState);

        public void SelectLocation(SceneName locationName) =>
            OnSelectLocationInnerEvent.Invoke(locationName);

        public void LoadSelectedLocation() =>
            OnLoadSelectedLocationInnerEvent.Invoke();

        public void AddPlayersGold(int amount) =>
            OnAddPlayersGoldInnerEvent.Invoke(amount);

        public void SpendPlayersGold(int amount) =>
            OnSpendPlayersGoldInnerEvent.Invoke(amount);

        public void ResetPlayersGold() =>
            OnResetPlayersGoldInnerEvent.Invoke();

        public SceneName GetSelectedLocation() =>
            OnGetSelectedLocationInnerEvent?.Invoke() ?? SceneName.Desert;

        public GameState GetGameState() =>
            OnGetGameStateInnerEvent?.Invoke() ?? GameState.WaitingForPlayers;
    }
}