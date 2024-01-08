using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EFPController
{

    [DefaultExecutionOrder(-997)]
    public class InputManagerLegacy : InputManager
    {

        public enum Axis
        {
            MovementX,
            MovementY,
            MouseLookX,
            MouseLookY,
            ControllerLookX,
            ControllerLookY,
        }

        [Serializable]
        public struct InputAxisData
        {
            public Axis axis;
            public string inputName;
        }

        [Serializable]
        public struct InputActionData
        {
            public Action action;
            public string inputName;
        }

        [SerializeField]
        private List<InputAxisData> axis = new List<InputAxisData>();
        [SerializeField]
        private List<InputActionData> actions = new List<InputActionData>();

        private void Update()
        {
            isGamepad = LookIsGamepad();
        }

        private float GetAxis(Axis _axis, bool raw = true)
        {
            if (axis.Exists(x => x.axis == _axis && !string.IsNullOrEmpty(x.inputName)))
            {
                InputAxisData iad = axis.First(x => x.axis == _axis);
                return raw ? Input.GetAxisRaw(iad.inputName) : Input.GetAxis(iad.inputName);
            }
            return 0f;
        }

        protected override Vector2 GetMouseLookInput()
        {
            return new Vector2(GetAxis(Axis.MouseLookX), GetAxis(Axis.MouseLookY));
        }

        protected override Vector2 GetControllerLookInput()
        {
            return new Vector2(GetAxis(Axis.ControllerLookX), GetAxis(Axis.ControllerLookY));
        }

        public override Vector2 GetMovementInput()
        {
            return new Vector2(GetAxis(Axis.MovementX), GetAxis(Axis.MovementY));
        }

        public override bool GetActionKey(Action action)
        {
            if (actions.Exists(x => x.action == action && !string.IsNullOrEmpty(x.inputName)))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return Input.GetButton(iad.inputName);
            }
            return false;
        }

        public override bool GetActionKeyUp(Action action)
        {
            if (actions.Exists(x => x.action == action && !string.IsNullOrEmpty(x.inputName)))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return Input.GetButtonUp(iad.inputName);
            }
            return false;
        }

        public override bool GetActionKeyDown(Action action)
        {
            if (actions.Exists(x => x.action == action && !string.IsNullOrEmpty(x.inputName)))
            {
                InputActionData iad = actions.First(x => x.action == action);
                return Input.GetButtonDown(iad.inputName);
            }
            return false;
        }

    }

}