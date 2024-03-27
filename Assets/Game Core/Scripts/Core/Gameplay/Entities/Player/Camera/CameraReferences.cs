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
        private Transform _itemPivot;
        
        [SerializeField, Required]
        private Transform _lookAtObject;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Camera MainCamera => _mainCamera;
        public Transform ItemPivot => _itemPivot;
        public Transform LookAtObject => _lookAtObject;
    }
}