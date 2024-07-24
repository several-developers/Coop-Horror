using System;
using System.Collections.Generic;
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
        private GameObject _doors;
        
        [SerializeField, Required]
        private Camera _outsideCamera;
        
        [SerializeField, Required]
        private MobileHQMainLever _mainLever;

        [SerializeField, Required]
        private SimpleButton _openQuestsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _openLocationsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _completeQuestsButton;
        
        [SerializeField, Required]
        private SimpleButton _loadMarketButton;

        [SerializeField, Required, Space(height: 5)]
        private List<MobileHQSeat> _allMobileHQSeats;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MoveSpeedController MoveSpeedController => _moveSpeedController;
        public Animator Animator => _animator;
        public AnimationObserver AnimationObserver => _animationObserver;
        public GameObject Doors => _doors;
        public Camera OutsideCamera => _outsideCamera;
        public MobileHQMainLever MainLever => _mainLever;
        public SimpleButton OpenQuestsSelectionMenuButton => _openQuestsSelectionMenuButton;
        public SimpleButton OpenLocationsSelectionMenuButton => _openLocationsSelectionMenuButton;
        public SimpleButton CompleteQuestsButton => _completeQuestsButton;
        public SimpleButton LoadMarketButton => _loadMarketButton;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<MobileHQSeat> GetAllMobileHQSeats() => _allMobileHQSeats;
    }
}