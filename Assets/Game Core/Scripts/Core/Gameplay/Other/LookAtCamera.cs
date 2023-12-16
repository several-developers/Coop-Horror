using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class LookAtCamera : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private Transform _cameraTransform;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Start() =>
            _cameraTransform = Camera.main.transform;

        private void LateUpdate() =>
            transform.rotation = _cameraTransform.rotation;
    }
}