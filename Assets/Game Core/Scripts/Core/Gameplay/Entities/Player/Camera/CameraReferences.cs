﻿using System;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.CameraManagement
{
    [Serializable]
    public class CameraReferences
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required]
        private Camera _mainCamera;
        
        [SerializeField, Required]
        private CinemachineBrain _cinemachineBrain;

        [SerializeField, Required]
        private GameObject _modelPivot;
        
        [SerializeField, Required]
        private Transform _leftHandItemsHolder;
        
        [SerializeField, Required]
        private Transform _rightHandItemsHolder;
        
        [SerializeField, Required]
        private Animator _playerArmsAnimator;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Camera MainCamera => _mainCamera;
        public CinemachineBrain CinemachineBrain => _cinemachineBrain;
        public GameObject ModelPivot => _modelPivot;
        public Transform LeftHandItemsHolder => _leftHandItemsHolder;
        public Transform RightHandItemsHolder => _rightHandItemsHolder;
        public Animator PlayerArmsAnimator => _playerArmsAnimator;
    }
}