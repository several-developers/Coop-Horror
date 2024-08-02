using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.GameManagement
{
    public class GameManagerDecorator : IGameManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<GameState> OnGameStateChangedEvent = delegate { };
        public event Action<LocationName> OnCurrentLocationChangedEvent = delegate { };
        public event Action<LocationName> OnSelectedLocationChangedEvent = delegate { };
        public event Action<int> OnPlayersGoldChangedEvent = delegate { };

        public event Action<GameState> OnChangeGameStateInnerEvent = delegate { };
        public event Action<GameState, GameState> OnChangeGameStateWhenAllPlayersReadyInnerEvent = delegate { };
        public event Action OnStartGameRestartTimerInnerEvent = delegate { };
        public event Action<LocationName> OnSelectLocationInnerEvent = delegate { };
        public event Action OnLoadSelectedLocationInnerEvent = delegate { };
        public event Action<int> OnAddPlayersGoldInnerEvent = delegate { };
        public event Action<int> OnSpendPlayersGoldInnerEvent = delegate { };
        public event Action OnResetPlayersGoldInnerEvent = delegate { };
        public event Func<LocationName> OnGetCurrentLocationInnerEvent;
        public event Func<LocationName> OnGetSelectedLocationInnerEvent;
        public event Func<LocationName> OnGetPreviousLocationInnerEvent;
        public event Func<GameState> OnGetGameStateInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void GameStateChanged(GameState gameState) =>
            OnGameStateChangedEvent.Invoke(gameState);

        public void CurrentLocationChanged(LocationName locationName) =>
            OnCurrentLocationChangedEvent.Invoke(locationName);

        public void SelectedLocationChanged(LocationName locationName) =>
            OnSelectedLocationChangedEvent.Invoke(locationName);

        public void PlayersGoldChanged(int playersGold) =>
            OnPlayersGoldChangedEvent.Invoke(playersGold);

        public void ChangeGameState(GameState gameState) =>
            OnChangeGameStateInnerEvent.Invoke(gameState);

        public void ChangeGameStateWhenAllPlayersReady(GameState newState, GameState previousState) =>
            OnChangeGameStateWhenAllPlayersReadyInnerEvent.Invoke(newState, previousState);

        public void StartGameRestartTimer() =>
            OnStartGameRestartTimerInnerEvent.Invoke();

        public void SelectLocation(LocationName locationName) =>
            OnSelectLocationInnerEvent.Invoke(locationName);

        public void LoadSelectedLocation() =>
            OnLoadSelectedLocationInnerEvent.Invoke();

        public void AddPlayersGold(int amount) =>
            OnAddPlayersGoldInnerEvent.Invoke(amount);

        public void SpendPlayersGold(int amount) =>
            OnSpendPlayersGoldInnerEvent.Invoke(amount);

        public void ResetPlayersGold() =>
            OnResetPlayersGoldInnerEvent.Invoke();

        public LocationName GetCurrentLocation() =>
            OnGetCurrentLocationInnerEvent?.Invoke() ?? LocationName.Base;

        public LocationName GetSelectedLocation() =>
            OnGetSelectedLocationInnerEvent?.Invoke() ?? LocationName.Base;

        public LocationName GetPreviousLocation() =>
            OnGetPreviousLocationInnerEvent?.Invoke() ?? LocationName.Base;

        public GameState GetGameState() =>
            OnGetGameStateInnerEvent?.Invoke() ?? GameState.WaitingForPlayers;
    }
}