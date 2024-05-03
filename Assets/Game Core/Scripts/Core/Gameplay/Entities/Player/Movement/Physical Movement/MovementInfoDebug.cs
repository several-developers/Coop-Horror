using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class MovementInfoDebug : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title("Debug Info")]
        [SerializeField]
        private Vector3 _currentVelocity;

        [SerializeField]
        private Vector3 _globalMoveDirection;

        [SerializeField]
        private bool _isGrounded;

        [SerializeField]
        private bool _performSprint;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Vector3 CurrentVelocity
        {
            set => _currentVelocity = value;
        }

        public Vector3 GlobalMoveDirection
        {
            set => _globalMoveDirection = value;
        }
        
        public bool IsGrounded
        {
            set => _isGrounded = value;
        }
        
        public bool PerformSprint
        {
            set => _performSprint = value;
        }
    }
}