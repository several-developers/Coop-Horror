﻿using System;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.MainMenu.OnlineMenu
{
    public class OnlineMenuView : MenuView
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _hostButton;
        
        [SerializeField, Required]
        private Button _joinButton;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnHostClickedEvent;
        public event Action OnJoinClickedEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _hostButton.onClick.AddListener(OnHostClicked);
            _joinButton.onClick.AddListener(OnJoinClicked);
            
            DestroyOnHide();
        }

        private void Start() => Show();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnHostClicked()
        {
            OnHostClickedEvent?.Invoke();
            Hide();
        }

        private void OnJoinClicked()
        {
            OnJoinClickedEvent?.Invoke();
            Hide();
        }
    }
}