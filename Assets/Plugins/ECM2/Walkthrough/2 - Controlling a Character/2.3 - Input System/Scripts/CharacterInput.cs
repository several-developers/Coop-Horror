using ECM2;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECM2.Walkthrough.Ex23
{
    /// <summary>
    /// This example shows how to make use of the new Input System,
    /// in particular, the PlayerInput component to control a character.
    /// </summary>
    
    public class CharacterInput : MonoBehaviour
    {
        /// <summary>
        /// Cached controlled character
        /// </summary>
        
        private Character _character;
        
        /// <summary>
        /// Movement InputAction event handler.
        /// </summary>

        public void OnMove(InputAction.CallbackContext context)
        {
            // Read input values

            Vector2 inputMovement = context.ReadValue<Vector2>();

            // Compose a movement direction vector in world space

            Vector3 movementDirection = Vector3.zero;

            movementDirection += Vector3.forward * inputMovement.y;
            movementDirection += Vector3.right * inputMovement.x;

            // If character has a camera assigned,
            // make movement direction relative to this camera view direction

            if (_character.camera)
            {               
                movementDirection 
                    = movementDirection.relativeTo(_character.cameraTransform);
            }
        
            // Set character's movement direction vector

            _character.SetMovementDirection(movementDirection);
        }

        /// <summary>
        /// Jump InputAction event handler.
        /// </summary>

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
                _character.Jump();
            else if (context.canceled)
                _character.StopJumping();
        }

        /// <summary>
        /// Crouch InputAction event handler.
        /// </summary>

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started)
                _character.Crouch();
            else if (context.canceled)
                _character.UnCrouch();
        }

        private void Awake()
        {
            //
            // Cache controlled character.
            
            _character = GetComponent<Character>();
        }
    }
}
