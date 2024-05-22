using Unity.Netcode;
using UnityEngine;

namespace ECM2.Examples.Networking.Netcode
{
    /// <summary>
    /// This example shows how to create a controller to implement client-authority character movement.
    /// </summary>

    public class PlayerController : NetworkBehaviour
    {
        private Character _character;

        private void Awake()
        {
            // Cache controlled Character

            _character = GetComponent<Character>();
        }

        private void Update()
        {
            // If not is owner, return

            if (!IsOwner)
                return;

            // If is owner, handle Character input

            Vector2 movementInput = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            Vector3 movementDirection = Vector3.zero;

            movementDirection += Vector3.forward * movementInput.y;
            movementDirection += Vector3.right * movementInput.x;

            _character.SetMovementDirection(movementDirection);

            if (Input.GetButtonDown("Jump"))
                _character.Jump();
            else if (Input.GetButtonUp("Jump"))
                _character.StopJumping();
        }
    }
}
