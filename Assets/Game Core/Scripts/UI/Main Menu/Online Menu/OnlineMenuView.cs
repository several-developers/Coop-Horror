using System;
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
        private Button _backButton;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            
        }
    }
}