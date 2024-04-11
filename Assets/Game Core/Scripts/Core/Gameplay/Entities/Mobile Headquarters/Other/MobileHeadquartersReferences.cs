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
        private ToggleMobileDoorLever _toggleMobileDoorLever;

        [SerializeField, Required]
        private LoadLocationLever _loadLocationLever;

        [SerializeField, Required]
        private LeaveLocationLever _leaveLocationLever;

        [SerializeField, Required]
        private SimpleButton _openQuestsSelectionMenuButton;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Animator Animator => _animator;
        public AnimationObserver AnimationObserver => _animationObserver;
        public ToggleMobileDoorLever ToggleMobileDoorLever => _toggleMobileDoorLever;
        public LoadLocationLever LoadLocationLever => _loadLocationLever;
        public LeaveLocationLever LeaveLocationLever => _leaveLocationLever;
        public SimpleButton OpenQuestsSelectionMenuButton => _openQuestsSelectionMenuButton;
    }
}