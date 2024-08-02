using System;
using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.GameManagement
{
    public interface IGameManagerDecorator
    {
        event Action<GameState> OnGameStateChangedEvent;
        event Action<LocationName> OnCurrentLocationChangedEvent;
        event Action<LocationName> OnSelectedLocationChangedEvent;
        event Action<int> OnPlayersGoldChangedEvent;
        void GameStateChanged(GameState gameState);
        void CurrentLocationChanged(LocationName locationName);
        void SelectedLocationChanged(LocationName locationName);
        void PlayersGoldChanged(int playersGold);
        
        event Action<GameState> OnChangeGameStateInnerEvent;
        event Action<GameState, GameState> OnChangeGameStateWhenAllPlayersReadyInnerEvent;
        event Action OnStartGameRestartTimerInnerEvent;
        event Action<LocationName> OnSelectLocationInnerEvent;
        event Action OnLoadSelectedLocationInnerEvent;
        event Action<int> OnAddPlayersGoldInnerEvent;
        event Action<int> OnSpendPlayersGoldInnerEvent;
        event Action OnResetPlayersGoldInnerEvent;
        event Func<LocationName> OnGetCurrentLocationInnerEvent;
        event Func<LocationName> OnGetSelectedLocationInnerEvent;
        event Func<LocationName> OnGetPreviousLocationInnerEvent;
        event Func<GameState> OnGetGameStateInnerEvent;
        /// <summary>
        /// State should be changed only once!
        /// </summary>
        void ChangeGameState(GameState gameState);
        void ChangeGameStateWhenAllPlayersReady(GameState newState, GameState previousState);
        void StartGameRestartTimer();
        void SelectLocation(LocationName locationName);
        void LoadSelectedLocation();
        void AddPlayersGold(int amount);
        void SpendPlayersGold(int amount);
        void ResetPlayersGold();
        LocationName GetCurrentLocation();
        LocationName GetSelectedLocation();
        LocationName GetPreviousLocation();
        GameState GetGameState();
    }
}