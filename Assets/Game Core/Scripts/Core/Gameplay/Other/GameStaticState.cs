namespace GameCore.Gameplay.Other
{
    public static class GameStaticState
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public static int SelectedSaveIndex { get; private set; } // Danger of incorrect value.
        public static bool IsMultiplayerEnabled { get; private set; }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void SetSelectedSaveIndex(int index) =>
            SelectedSaveIndex = index;

        public static void ToggleMultiplayer(bool isEnabled) =>
            IsMultiplayerEnabled = isEnabled;
    }
}