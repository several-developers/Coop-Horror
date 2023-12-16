using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Other
{
    public class RagdollEnabler : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private Transform _ragdollRoot;

        [SerializeField, Required, Space(height: 5)]
        private Rigidbody[] _rigidbodies;

        [SerializeField, Required]
        private Collider[] _colliders;

        [SerializeField, Required]
        private CharacterJoint[] _characterJoints;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => EnableAnimator();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void EnableRagdoll()
        {
            _animator.enabled = false;

            foreach (CharacterJoint joint in _characterJoints)
                joint.enableCollision = true;

            foreach (Collider collider3d in _colliders)
                collider3d.enabled = true;

            foreach (Rigidbody rbody in _rigidbodies)
            {
                rbody.velocity = Vector3.zero;
                rbody.detectCollisions = true;
                rbody.useGravity = true;
            }
        }

        public void EnableAnimator()
        {
            _animator.enabled = true;

            foreach (CharacterJoint joint in _characterJoints)
                joint.enableCollision = false;

            foreach (Collider collider3d in _colliders)
                collider3d.enabled = false;

            foreach (Rigidbody rbody in _rigidbodies)
            {
                rbody.detectCollisions = false;
                rbody.useGravity = false;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [Button(buttonSize: 25), DisableInPlayMode]
        private void FindAllResources()
        {
            _rigidbodies = _ragdollRoot.GetComponentsInChildren<Rigidbody>();
            _colliders = _ragdollRoot.GetComponentsInChildren<Collider>();
            _characterJoints = _ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        }

        // DEBUG BUTTONS: -------------------------------------------------------------------------

#if UNITY_EDITOR
        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugEnableRagdoll() => EnableRagdoll();

        [Button(buttonSize: 25), DisableInEditorMode]
        private void DebugEnableAnimator() => EnableAnimator();
#endif
    }
}