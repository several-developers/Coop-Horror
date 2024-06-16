using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.InputManagement
{
    [CreateAssetMenu(menuName = "Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions, GameInput.IMenusActions
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public GameInput GameInput => _gameInput;
        public bool MovementInputDetected => _movementInputDetected;

        // FIELDS: --------------------------------------------------------------------------------

        // Gameplay
        public event Action<Vector2> OnMoveEvent = delegate { };
        public event Action OnSprintEvent = delegate { };
        public event Action OnSprintCanceledEvent = delegate { };
        public event Action OnJumpEvent = delegate { };
        public event Action OnJumpCanceledEvent = delegate { };
        public event Action OnCrouchEvent = delegate { };
        public event Action OnCrouchCanceledEvent = delegate { };
        public event Action<Vector2> OnLookEvent = delegate { };
        public event Action OnAttackEvent = delegate { };
        public event Action OnAttackCanceledEvent = delegate { };
        public event Action OnInteractEvent = delegate { };
        public event Action OnDropItemEvent = delegate { };
        public event Action<float> OnScrollEvent = delegate { };
        public event Action OnPauseEvent = delegate { };
        public event Action OnInteractWithTabletEvent = delegate { };
        public event Action OnOpenChatEvent = delegate { };

        // Menus
        public event Action OnResumeEvent = delegate { };
        public event Action OnSubmitEvent = delegate { };

        private GameInput _gameInput;

        private bool _movementInputDetected;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void OnEnable()
        {
            if (_gameInput != null)
                return;

            _gameInput = new GameInput();

            _gameInput.Gameplay.SetCallbacks(instance: this);
            _gameInput.Menus.SetCallbacks(instance: this);

            EnableGameplayInput();
        }

        private void OnDisable() => DisableAllInput();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void EnableGameplayInput()
        {
            // Check if nessesary.
            //_playerInputActions.SwitchCurrentActionMap(Constants.PlayerActionMap);

            _gameInput.Gameplay.Enable();
            _gameInput.Menus.Disable();
        }

        public void EnableUIInput()
        {
            _gameInput.Gameplay.Disable();
            _gameInput.Menus.Enable();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DisableAllInput()
        {
            _gameInput.Gameplay.Disable();
            _gameInput.Menus.Disable();
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnMove(InputAction.CallbackContext context)
        {
            var moveVector = context.ReadValue<Vector2>();
            _movementInputDetected = moveVector.magnitude > 0f;
            OnMoveEvent.Invoke(moveVector);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    OnSprintEvent.Invoke();
                    break;

                case InputActionPhase.Canceled:
                    OnSprintCanceledEvent.Invoke();
                    break;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    OnJumpEvent.Invoke();
                    break;
                
                case InputActionPhase.Canceled:
                    OnJumpCanceledEvent.Invoke();
                    break;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    OnCrouchEvent.Invoke();
                    break;
                
                case InputActionPhase.Canceled:
                    OnCrouchCanceledEvent.Invoke();
                    break;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            var lookVector = context.ReadValue<Vector2>();
            OnLookEvent.Invoke(lookVector);
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    OnAttackEvent.Invoke();
                    break;

                case InputActionPhase.Canceled:
                    OnAttackCanceledEvent.Invoke();
                    break;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractEvent.Invoke();
        }

        public void OnDropItem(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnDropItemEvent.Invoke();
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            var scrollValue = context.ReadValue<float>();
            OnScrollEvent.Invoke(scrollValue);
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            OnPauseEvent.Invoke();
            EnableUIInput();
        }

        public void OnInteractWithTablet(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            OnInteractWithTabletEvent.Invoke();
        }

        public void OnOpenChat(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnOpenChatEvent.Invoke();
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            OnSubmitEvent.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
        }

        public void OnResume(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            OnResumeEvent.Invoke();
            //EnableGameplayInput();
        }
    }
}