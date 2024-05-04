using ECM2;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    /// <summary>
    /// This example shows how to animate a Character,
    /// using the Character data (movement direction, velocity, is jumping, etc) to feed your Animator.
    /// </summary>
    public class MyAnimationController : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        //private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");
        
        private Character _character;
        private MySprintAbility _sprintAbility;
        private bool _isInitialized;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Update()
        {
            if (!_isInitialized)
                return;
            
            float deltaTime = Time.deltaTime;

            // Get Character animator

            Animator animator = _character.GetAnimator();

            // Compute input move vector in local space

            Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());

            // Update the animator parameters

            float forwardAmount = _character.useRootMotion && _character.GetRootMotionController()
                ? move.z
                : Mathf.InverseLerp(a: 0.0f, b: _character.GetMaxSpeed(), value: _character.GetSpeed());

            bool isMovingBackwards = move.z < 0f;

            if (isMovingBackwards)
                forwardAmount *= -1f;
            else
                animator.SetFloat(id: AnimatorHashes.Turn, value: Mathf.Atan2(y: move.x, x: move.z), dampTime: 0.15f, deltaTime);

            animator.SetFloat(id: AnimatorHashes.Forward, value: forwardAmount, dampTime: 0.15f, deltaTime);
            animator.SetFloat(id: AnimatorHashes.Sprint, value: _sprintAbility.IsSprinting() ? forwardAmount : 0.0f, dampTime: 0.3f, deltaTime);

            animator.SetBool(id: AnimatorHashes.Ground, value: _character.IsGrounded());
            animator.SetBool(id: AnimatorHashes.Crouch, value: _character.IsCrouched());

            if (_character.IsFalling())
                animator.SetFloat(id: AnimatorHashes.Jump, value: _character.GetVelocity().y, dampTime: 0.1f, deltaTime);

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            //float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            //float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            // if (_character.IsGrounded())
            //     animator.SetFloat(JumpLeg, jumpLeg);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(Character character)
        {
            _character = character;
            _sprintAbility = character.GetComponent<MySprintAbility>();
            _isInitialized = true;
        }
    }
}