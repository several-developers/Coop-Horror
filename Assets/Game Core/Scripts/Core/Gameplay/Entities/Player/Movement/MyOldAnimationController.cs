using ECM2;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player
{
    /// <summary>
    /// This example shows how to animate a Character,
    /// using the Character data (movement direction, velocity, is jumping, etc) to feed your Animator.
    /// </summary>
    public class MyOldAnimationController : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private Character _character;
        private MySprintAbility _sprintAbility;
        private Animator _animator;
        private Animator _armsAnimator;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        /*private void Update()
        {
            float deltaTime = Time.deltaTime;

            // Compute input move vector in local space

            Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());

            // Update the animator parameters

            float forwardAmount = _character.useRootMotion && _character.GetRootMotionController()
                ? move.z
                : Mathf.InverseLerp(a: 0.0f, b: _character.GetMaxSpeed(), value: _character.GetSpeed());

            bool isMovingBackwards = move.z < 0f;

            if (!isMovingBackwards)
            {
#warning НЕПРАВИЛЬНО РАБОТАЕТ, в правую сторону -1 даёт
                float turnAmount = Mathf.Atan2(y: move.x, x: move.z);

                _animator.SetFloat(id: AnimatorHashes.Turn, value: turnAmount, dampTime: 0.15f, deltaTime);
                _armsAnimator.SetFloat(id: AnimatorHashes.Turn, value: turnAmount, dampTime: 0.15f, deltaTime);
            }
            else
                forwardAmount *= -1f;

            float sprintAmount = _sprintAbility.IsSprinting() ? forwardAmount : 0.0f;
            bool isGrounded = _character.IsGrounded();
            bool isCrouched = _character.IsCrouched();

            _animator.SetFloat(id: AnimatorHashes.Forward, value: forwardAmount, dampTime: 0.15f, deltaTime);
            _animator.SetFloat(id: AnimatorHashes.Sprint, value: sprintAmount, dampTime: 0.3f, deltaTime);

            _armsAnimator.SetFloat(id: AnimatorHashes.Forward, value: forwardAmount, dampTime: 0.15f, deltaTime);
            _armsAnimator.SetFloat(id: AnimatorHashes.Sprint, value: sprintAmount, dampTime: 0.3f, deltaTime);

            _animator.SetBool(id: AnimatorHashes.Ground, value: isGrounded);
            _animator.SetBool(id: AnimatorHashes.Crouch, value: isCrouched);

            _armsAnimator.SetBool(id: AnimatorHashes.Ground, value: isGrounded);
            _armsAnimator.SetBool(id: AnimatorHashes.Crouch, value: isCrouched);

            if (_character.IsFalling())
            {
                float yVelocity = _character.GetVelocity().y;

                _animator.SetFloat(id: AnimatorHashes.Jump, value: yVelocity, dampTime: 0.1f, deltaTime);
                _armsAnimator.SetFloat(id: AnimatorHashes.Jump, value: yVelocity, dampTime: 0.1f, deltaTime);
            }

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            //float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            //float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            // if (_character.IsGrounded())
            //     animator.SetFloat(JumpLeg, jumpLeg);
        }*/
    }
}