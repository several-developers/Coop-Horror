namespace GameCore.Gameplay.Other
{
    public static class GameSingleton
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public static int SelectedSaveIndex { get; private set; } // Danger of incorrect value.

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void SetSelectedSaveIndex(int index) =>
            SelectedSaveIndex = index;
    }
}