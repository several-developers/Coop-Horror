using UnityEngine;

namespace GameCore.Gameplay.Network.Other
{
    public class FollowParent : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private Transform _transform;
        private Transform _target;
        private Vector3 _rotationOffset;
        private bool _isTargetFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _transform = transform;

        private void LateUpdate()
        {
            if (!_isTargetFound)
                return;

            _transform.position = _target.position;
            _transform.rotation = _target.rotation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetTarget(Transform followTarget)
        {
            _target = followTarget;
            _isTargetFound = true;
        }

        public void RemoveTarget()
        {
            _target = null;
            _isTargetFound = false;
        }
    }
}