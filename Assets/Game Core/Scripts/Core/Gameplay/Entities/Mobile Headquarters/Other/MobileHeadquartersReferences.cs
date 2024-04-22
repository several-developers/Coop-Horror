using System;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.MobileHeadquarters;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.MobileHeadquarters
{
    [Serializable]
    public class MobileHeadquartersReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;

        [SerializeField, Required]
        private SimpleButton _openQuestsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _openLocationsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _callDeliveryDroneButton;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Animator Animator => _animator;
        public AnimationObserver AnimationObserver => _animationObserver;
        public LoadLocationLever LoadLocationLever => _loadLocationLever;
        public LeaveLocationLever LeaveLocationLever => _leaveLocationLever;
        public SimpleButton OpenQuestsSelectionMenuButton => _openQuestsSelectionMenuButton;
        public SimpleButton OpenLocationsSelectionMenuButton => _openLocationsSelectionMenuButton;
        public SimpleButton CallDeliveryDroneButton => _callDeliveryDroneButton;
    }
}