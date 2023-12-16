namespace GameCore
{
    public static class EditorConstants
    {
        // FIELDS: --------------------------------------------------------------------------------

        public const string ConfigsCategory = "Configs";
        public const string PlayerConfigsCategory = ConfigsCategory + "/Player";
        public const string LevelsCategory = "Levels";
        public const string WeaponsCategory = "Weapons";
        public const string CollectionsCategory = "Collections";

        public const string GameData = Settings + "GameData";
        public const string GameSettings = Settings + "GameSettings";
        public const string PlayerData = Settings + "PlayerData";
        
        public const string MetaSettings = "Meta Settings";
        public const string MetaSettingsIn = "Meta Settings/In";
        public const string NoCategory = "No category";
        
        private const string Settings = "Settings/";
    }
}