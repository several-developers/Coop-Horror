using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EFPController
{
    [DefaultExecutionOrder(-997)]
    public class InputManagerNew : InputManager
    {
        public enum Axis
        {
            Movement,
            MouseLook,
            ControllerLook,
        }

        [Serializable]
        public struct InputAxisData
        {
            public Axis axis;
            public InputActionReference inputAction;
        }

        [Serializable]
        public struct InputActionData
        {
            public Action action;
            public InputActionReference inputAction;
        }

        [SerializeField]
        private List<InputAxisData> axis = new List<InputAxisData>();

        [SerializeField]
        private List<InputActionData> actions = new List<InputActionData>();

        void Start()
        {
            InputSystem.onEvent += (eventPtr, device) =>
            {
                if (device != null) isGamepad = device is Gamepad;
            };

            foreach (InputAxisData axisData in axis)
            {
                if (axisData.inputAction != null) axisData.inputAction.action.Enable();
            }

            foreach (InputActionData actionData in actions)
            {
                if (actionData.inputAction != null) actionData.inputAction.action.Enable();
            }
        }

        protected override Vector2 GetMouseLookInput()
        {
            if (axis.Exists(x => x.axis == Axis.MouseLook && x.inputAction != null))
            {
                InputAxisData iad = axis.First(x => x.axis == Axis.MouseLook);
                return iad.inputAction.action.ReadValue<Vector2>();
            }

            return Vector2.zero;
        }

        protected override Vector2 GetControllerLookInput()
        {
            if (axis.Exists(x => x.axis == Axis.ControllerLook && x.inputAction != null))
            {
                InputAxisData iad = axis.First(x => x.axis == Axis.ControllerLook);
                return iad.inputAction.action.ReadValue<Vector2>();
            }

            return Vector2.zero;
        }

        public override Vector2 GetMovementInput()
        {
            if (axis.Exists(x => x.axis == Axis.Movement && x.inputAction != null))
            {
                InputAxisData iad = axis.First(x => x.axis == Axis.Movement);
                return iad.inputAction.action.ReadValue<Vector2>();
            }

            return Vector2.zero;
        }

        // IsPressed
        public override bool GetActionKey(Action action)
        {
            if (actions.Exists(x => x.action == action && x.inputAction != null))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return iad.inputAction.action.IsPressed();
            }

            return false;
        }

        // WasReleasedThisFrame
        public override bool GetActionKeyUp(Action action)
        {
            if (actions.Exists(x => x.action == action && x.inputAction != null))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return iad.inputAction.action.WasReleasedThisFrame();
            }

            return false;
        }

        // WasPerformedThisFrame
        public override bool GetActionKeyDown(Action action)
        {
            if (actions.Exists(x => x.action == action && x.inputAction != null))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return iad.inputAction.action.WasPerformedThisFrame();
            }

            return false;
        }
    }
}