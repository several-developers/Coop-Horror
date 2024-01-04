using System;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.NewMovement
{
    public class MyInputManager : MonoBehaviour
    {
        // FIELDS: --------------------------------------------------------------------------------

        private PlayerInputActions _playerInputActions;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => Init();

        private void OnDestroy()
        {
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public Vector2 GetMovementInput() =>
            _playerInputActions.Player.Move.ReadValue<Vector2>();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Init()
        {
            _playerInputActions = new PlayerInputActions();

            _playerInputActions.Player.Enable();
        }
    }
}