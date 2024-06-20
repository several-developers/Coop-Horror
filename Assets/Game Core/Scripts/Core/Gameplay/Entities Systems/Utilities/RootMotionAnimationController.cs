using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.Utilities
{
    public class RootMotionAnimationController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private NavMeshAgent _agent;

        // FIELDS: --------------------------------------------------------------------------------

        private Transform _transform;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _transform = transform;

        private void OnAnimatorMove()
        {
            Vector3 rootPosition = _animator.rootPosition;
            rootPosition.y = _agent.nextPosition.y;
            
            _transform.position = rootPosition;
            _transform.rotation = _animator.rootRotation;
            _animator.rootPosition = rootPosition;
        }
    }
}