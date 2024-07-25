using System;
using System.Collections.Generic;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Interactable.Train;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Train
{
    [Serializable]
    public class TrainReferences
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
        private TrainMainLever _mainLever;

        [SerializeField, Required]
        private SimpleButton _openQuestsSelectionMenuButton;
        
        [SerializeField, Required]
        private SimpleButton _completeQuestsButton;
        
        [SerializeField, Required]
        private SimpleButton _loadMarketButton;

        [SerializeField, Required, Space(height: 5)]
        private List<TrainSeat> _allMobileHQSeats;

        // PROPERTIES: ----------------------------------------------------------------------------

        public MoveSpeedController MoveSpeedController => _moveSpeedController;
        public Animator Animator => _animator;
        public AnimationObserver AnimationObserver => _animationObserver;
        public GameObject Doors => _doors;
        public Camera OutsideCamera => _outsideCamera;
        public TrainMainLever MainLever => _mainLever;
        public SimpleButton OpenQuestsSelectionMenuButton => _openQuestsSelectionMenuButton;
        public SimpleButton CompleteQuestsButton => _completeQuestsButton;
        public SimpleButton LoadMarketButton => _loadMarketButton;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public IReadOnlyList<TrainSeat> GetAllMobileHQSeats() => _allMobileHQSeats;
    }
}