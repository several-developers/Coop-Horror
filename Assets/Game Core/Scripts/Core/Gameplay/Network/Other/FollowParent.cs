using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Network.Other
{
    public class FollowParent : NetworkBehaviour
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
            if (!IsOwner)
                return;

            if (!_isTargetFound)
                return;

            _transform.position = _target.position;
            _transform.rotation = _target.rotation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTarget(Transform followTarget)
        {
            ChangeTarget(followTarget);

            _isTargetFound = true;
        }

        public void ChangeTarget(Transform followTarget) =>
            _target = followTarget;

        public void RemoveTarget()
        {
            _target = null;
            _isTargetFound = false;
        }
    }
}