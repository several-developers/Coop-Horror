using UnityEngine;

namespace EFPController
{

    [DefaultExecutionOrder(-997)]
    public abstract class InputManager : MonoBehaviour
    {
        public static InputManager instance;

        public enum Action
        {
            Sprint,
            Jump,
            Crouch,
            Interact,
            LeanLeft,
            LeanRight,
            Dash,
        }

        [Tooltip("Reverse vertical input for look.")]
        public bool invertVerticalLook;

        public static bool isGamepad { get; protected set; }

        private void Awake()
        {
            instance = this;
        }

        public static void ShowCursor(bool value)
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        }

        protected abstract Vector2 GetMouseLookInput();

        protected abstract Vector2 GetControllerLookInput();

        public Vector2 GetLookInput(bool useInvert = true)
        {
            Vector2 value = GetMouseLookInput();
            if (value.sqrMagnitude == 0f)
            {
                value = GetControllerLookInput() * 100f;
            }
            if (invertVerticalLook && useInvert)
            {
                value.y = -value.y;
            }
            return value;
        }

        public bool LookIsGamepad()
        {
            Vector2 value = GetControllerLookInput();
            return value.sqrMagnitude > 0f;
        }

        public abstract Vector2 GetMovementInput();

        public abstract bool GetActionKey(Action action);
        public abstract bool GetActionKeyUp(Action action);
        public abstract bool GetActionKeyDown(Action action);

    }

}