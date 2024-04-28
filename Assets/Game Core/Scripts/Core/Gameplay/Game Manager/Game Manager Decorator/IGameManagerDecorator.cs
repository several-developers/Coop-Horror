using System;
using GameCore.Enums.Gameplay;
using GameCore.Enums.Global;

namespace GameCore.Gameplay.GameManagement
{
    public interface IGameManagerDecorator
    {
        event Action<GameState> OnGameStateChangedEvent;
        event Action<SceneName> OnSelectedLocationChangedEvent;
        event Action<int> OnPlayersGoldChangedEvent;
        void GameStateChanged(GameState gameState);
        void SelectedLocationChanged(SceneName locationName);
        void PlayersGoldChanged(int playersGold);
        
        event Action<GameState> OnChangeGameStateInnerEvent;
        event Action<SceneName> OnSelectLocationInnerEvent;
        event Action OnLoadSelectedLocationInnerEvent;
        event Action<int> OnAddPlayersGoldInnerEvent;
        event Action<int> OnSpendPlayersGoldInnerEvent;
        event Func<SceneName> OnGetSelectedLocationInnerEvent;
        event Func<GameState> OnGetGameStateInnerEvent;
        /// <summary>
        /// State should be changed only once!
        /// </summary>
        void ChangeGameState(GameState gameState);
        void SelectLocation(SceneName locationName);
        void LoadSelectedLocation();
        void AddPlayersGold(int amount);
        void SpendPlayersGold(int amount);
        SceneName GetSelectedLocation();
        GameState GetGameState();
    }
}