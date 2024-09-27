using System;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.LobbiesMenu.RelayLobby
{
    /// <summary>
    /// An individual Lobby UI in the list of available lobbies
    /// </summary>
    public class LobbyListItemUI : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _button;
        
        [SerializeField]
        private TextMeshProUGUI _lobbyNameTMP;
        
        [SerializeField]
        private TextMeshProUGUI _lobbyCountTMP;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<LocalLobby> OnClickedEvent;
        
        private LocalLobby _data;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _button.onClick.AddListener(OnButtonClicked);

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetData(LocalLobby data)
        {
            _data = data;
            _lobbyNameTMP.SetText(data.LobbyName);
            _lobbyCountTMP.SetText($"{data.PlayerCount}/{data.MaxPlayerCount}");
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        private void OnButtonClicked() =>
            OnClickedEvent?.Invoke(_data);
    }
}