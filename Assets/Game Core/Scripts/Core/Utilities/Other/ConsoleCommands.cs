using IngameDebugConsole;
using UnityEngine;

namespace GameCore.Utilities
{
    public static class ConsoleCommands
    {
        [ConsoleMethod("cursor", "Changes cursor lock state.")]
        public static void ChangeCursorLockState()
        {
            CursorLockMode cursorLockMode = Cursor.lockState;

            Cursor.lockState = cursorLockMode switch
            {
                CursorLockMode.Locked => CursorLockMode.None,
                CursorLockMode.None => CursorLockMode.Locked,
                _ => Cursor.lockState
            };
        }
    }
}