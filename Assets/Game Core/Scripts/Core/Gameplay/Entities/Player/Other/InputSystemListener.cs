using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.Entities.Player
{
    public class InputSystemListener : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMoveEvent;
        public event Action<Vector2> OnLookEvent;
        public event Action OnChangeCursorStateEvent;
        public event Action OnChangeCameraLockStateEvent;
        public event Action OnShootEvent;
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnMove(InputAction.CallbackContext context)
        {
            var movementVector = context.ReadValue<Vector2>();
            OnMoveEvent?.Invoke(movementVector);
        }
        
        public void OnLook(InputAction.CallbackContext context)
        {
            var lookVector = context.ReadValue<Vector2>();
            OnLookEvent?.Invoke(lookVector);
        }
        
        public void OnChangeCursorState(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnChangeCursorStateEvent?.Invoke();
        }
        
        public void OnCameraLockState(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnChangeCameraLockStateEvent?.Invoke();
        }
        
        public void OnShoot(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnShootEvent?.Invoke();
        }
    }
}