using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCore.Gameplay.Managers
{
    public static class InputSystemManager
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public static event Action OnOpenPauseMenuEvent;
        public static event Action<Vector2> OnMoveEvent;
        public static event Action<Vector2> OnLookEvent;
        public static event Action<float> OnScrollEvent;
        public static event Action OnInteractEvent;
        public static event Action OnDropItemEvent;

        private static PlayerInputActions _playerInputActions;
        private static PlayerInput _playerInput;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        // НЕ РАБОТАЕТ НА СЕРВЕРЕ, У КАЖДОГО КЛИЕНТА ДОЛЖЕН БЫТЬ СВОЙ ИНПУТ
        public static void Init(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _playerInputActions = new PlayerInputActions();

            _playerInputActions.Player.Enable();
            
            _playerInputActions.Player.OpenPauseMenu.performed += OpenPauseMenu;
            _playerInputActions.Player.Move.performed += OnMove;
            _playerInputActions.Player.Look.performed += OnLook;
            _playerInputActions.Player.Scroll.performed += OnScroll;
            _playerInputActions.Player.Interact.performed += OnInteract;
            _playerInputActions.Player.DropItem.performed += OnDropItem;
        }

        public static void SwitchToPlayer()
        {
            _playerInput.SwitchCurrentActionMap(Constants.PlayerActionMap);
            _playerInputActions.Player.Enable();
            _playerInputActions.UI.Disable();
        }

        public static void SwitchToUI()
        {
            _playerInput.SwitchCurrentActionMap(Constants.UIActionMap);
            _playerInputActions.Player.Disable();
            _playerInputActions.UI.Enable();
        }

        public static PlayerInputActions GetPlayerInputActions() => _playerInputActions;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private static void OpenPauseMenu(InputAction.CallbackContext context) =>
            OnOpenPauseMenuEvent?.Invoke();
        
        private static void OnMove(InputAction.CallbackContext context)
        {
            var movementVector = context.ReadValue<Vector2>();
            OnMoveEvent?.Invoke(movementVector);
        }
        
        private static void OnLook(InputAction.CallbackContext context)
        {
            var lookVector = context.ReadValue<Vector2>();
            OnLookEvent?.Invoke(lookVector);
        }
        
        private static void OnScroll(InputAction.CallbackContext context)
        {
            var scrollValue = context.ReadValue<float>();
            OnScrollEvent?.Invoke(scrollValue);
        }
        
        private static void OnInteract(InputAction.CallbackContext context) =>
            OnInteractEvent?.Invoke();
        
        private static void OnDropItem(InputAction.CallbackContext context) =>
            OnDropItemEvent?.Invoke();
    }
}