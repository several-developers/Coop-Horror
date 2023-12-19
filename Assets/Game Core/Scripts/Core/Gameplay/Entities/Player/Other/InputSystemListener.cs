using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.Entities.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputSystemListener : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMoveEvent;
        public event Action<Vector2> OnLookEvent;
        public event Action OnChangeCursorStateEvent;
        public event Action OnChangeCameraLockStateEvent;
        public event Action OnShootEvent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            var playerInputActions = new PlayerInputActions();

            playerInputActions.Player.Enable(); // For player, for ui u need another
            playerInputActions.Player.Move.performed += OnMove;
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(InputAction.CallbackContext context)
        {
            var movementVector = context.ReadValue<Vector2>();
            OnMoveEvent?.Invoke(movementVector);
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            var lookVector = context.ReadValue<Vector2>();
            OnLookEvent?.Invoke(lookVector);
        }
        
        private void OnChangeCursorState(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnChangeCursorStateEvent?.Invoke();
        }
        
        private void OnCameraLockState(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnChangeCameraLockStateEvent?.Invoke();
        }
        
        private void OnShoot(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnShootEvent?.Invoke();
        }
    }
}