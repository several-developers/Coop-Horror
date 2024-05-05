using System;
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
        private Transform _leftHandItemsHolder;
        
        [SerializeField, Required]
        private Transform _rightHandItemsHolder;
        
        [SerializeField, Required]
        private Transform _lookAtObject;

        [SerializeField, Required]
        private Animator _playerArmsAnimator;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Camera MainCamera => _mainCamera;
        public Transform LeftHandItemsHolder => _leftHandItemsHolder;
        public Transform RightHandItemsHolder => _rightHandItemsHolder;
        public Transform LookAtObject => _lookAtObject;
        public Animator PlayerArmsAnimator => _playerArmsAnimator;
    }
}