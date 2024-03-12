using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameCore.Gameplay.Network.UnityServices.Lobbies;
using GameCore.Gameplay.PubSub;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.MainMenu.LobbiesMenu.RelayLobby
{
    /// <summary>
    /// Handles the list of LobbyListItemUIs and ensures it stays synchronized with the lobby list from the service.
    /// </summary>
    public class LobbyJoiningUI : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ISubscriber<LobbyListFetchedMessage> localLobbiesRefreshedSub,
            IUpdateRunner updateRunner)
        {
            _localLobbiesRefreshedSub = localLobbiesRefreshedSub;
            _updateRunner = updateRunner;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Transform _lobbyItemsContainer;
        
        [SerializeField, Required]
        private LobbyListItemUI _lobbyListItemPrefab;

        [SerializeField, Required]
        private TMP_InputField _joinCodeIF;
        
        [SerializeField, Required]
        private GameObject _emptyLobbyListLabel;

        [SerializeField, Required]
        private Button _joinLobbyButton;
        
        [SerializeField, Required]
        private Button _quickJoinLobbyButton;

        [SerializeField, Required]
        private Button _refreshLobbyButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<string> OnJoinClickedEvent;
        public event Action OnQuickJoinClickedEvent;
        public event Action<bool> OnRefreshLobbyEvent;
        public event Action<LocalLobby> OnLobbyItemClickedEvent;

        private readonly List<LobbyListItemUI> _lobbyListItems = new();
        
        private ISubscriber<LobbyListFetchedMessage> _localLobbiesRefreshedSub;
        private IUpdateRunner _updateRunner;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _localLobbiesRefreshedSub.Subscribe(UpdateUI);
            
            _joinCodeIF.onValueChanged.AddListener(OnJoinCodeInputTextChanged);
            _joinLobbyButton.onClick.AddListener(OnJoinClicked);
            _quickJoinLobbyButton.onClick.AddListener(OnQuickJoinClicked);
            _refreshLobbyButton.onClick.AddListener(OnRefreshLobbyClicked);
        }

        private void OnDestroy() =>
            _localLobbiesRefreshedSub?.Unsubscribe(UpdateUI);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Show()
        {
            _joinCodeIF.text = "";
            _updateRunner.Subscribe(PeriodicRefresh, updatePeriod: 10f);
        }

        public void Hide() =>
            _updateRunner.Unsubscribe(PeriodicRefresh);

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateUI(LobbyListFetchedMessage message)
        {
            EnsureNumberOfActiveUISlots(message.LocalLobbies.Count);

            for (var i = 0; i < message.LocalLobbies.Count; i++)
            {
                LocalLobby localLobby = message.LocalLobbies[i];
                _lobbyListItems[i].SetData(localLobby);
            }

            if (message.LocalLobbies.Count == 0)
                _emptyLobbyListLabel.SetActive(true);
            else
                _emptyLobbyListLabel.SetActive(false);
        }
        
        private void EnsureNumberOfActiveUISlots(int requiredNumber)
        {
            int delta = requiredNumber - _lobbyListItems.Count;

            for (int i = 0; i < delta; i++)
                _lobbyListItems.Add(CreateLobbyListItem());

            for (int i = 0; i < _lobbyListItems.Count; i++)
                _lobbyListItems[i].gameObject.SetActive(i < requiredNumber);
        }

        // This is a soft refresh without needing to lock the UI and such.
        private void PeriodicRefresh(float _) =>
            OnRefreshLobbyEvent?.Invoke(false);
        
        private LobbyListItemUI CreateLobbyListItem()
        {
            LobbyListItemUI listItem = Instantiate(_lobbyListItemPrefab, _lobbyItemsContainer);
            listItem.gameObject.SetActive(true);
            listItem.OnClickedEvent += OnLobbyListItemClicked;
            
            return listItem;
        }

        private static string SanitizeJoinCode(string dirtyString) =>
            Regex.Replace(input: dirtyString.ToUpper(), pattern: "[^A-Z0-9]", replacement: "");

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnJoinCodeInputTextChanged(string text)
        {
            _joinCodeIF.text = SanitizeJoinCode(text);
            _joinLobbyButton.interactable = _joinCodeIF.text.Length > 0;
        }

        private void OnJoinClicked()
        {
            string joinCode = SanitizeJoinCode(_joinCodeIF.text);
            OnJoinClickedEvent?.Invoke(joinCode);
        }

        private void OnQuickJoinClicked() =>
            OnQuickJoinClickedEvent?.Invoke();

        private void OnRefreshLobbyClicked() =>
            OnRefreshLobbyEvent?.Invoke(true);

        private void OnLobbyListItemClicked(LocalLobby data) =>
            OnLobbyItemClickedEvent?.Invoke(data);
    }
}