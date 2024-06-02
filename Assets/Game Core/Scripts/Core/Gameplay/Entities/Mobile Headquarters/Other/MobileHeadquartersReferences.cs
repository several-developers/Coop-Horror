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
        private MoveSpeedController _moveSpeedController;
        
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        [SerializeField, Required]
        private Camera _outsideCamera;
        
        [SerializeField, Required]
        private MobileHQMainLever _mainLever;

        [SerializeField, Required]
        private SimpleButton _openQuestsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _openLocationsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _callDeliveryDroneButton;
        
        [SerializeField, Required]
        private SimpleButton _completeQuestsButton;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MoveSpeedController MoveSpeedController => _moveSpeedController;
        public Animator Animator => _animator;
        public AnimationObserver AnimationObserver => _animationObserver;
        public Camera OutsideCamera => _outsideCamera;
        public MobileHQMainLever MainLever => _mainLever;
        public SimpleButton OpenQuestsSelectionMenuButton => _openQuestsSelectionMenuButton;
        public SimpleButton OpenLocationsSelectionMenuButton => _openLocationsSelectionMenuButton;
        public SimpleButton CallDeliveryDroneButton => _callDeliveryDroneButton;
        public SimpleButton CompleteQuestsButton => _completeQuestsButton;
    }
}