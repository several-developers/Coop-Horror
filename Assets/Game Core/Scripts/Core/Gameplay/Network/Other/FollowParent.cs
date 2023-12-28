using UnityEngine;

namespace GameCore.Gameplay.Network.Other
{
    public class FollowParent : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private Transform _transform;
        private Transform _target;
        private bool _isTargetFound;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void LateUpdate()
        {
            if (!_isTargetFound)
                return;

            _transform.position = _target.position;
            _transform.rotation = _target.rotation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Transform target)
        {
            _transform = transform;
            _target = target;
            _isTargetFound = true;
        }

        public void RemoveTarget()
        {
            _target = null;
            _isTargetFound = false;
        }
    }
}