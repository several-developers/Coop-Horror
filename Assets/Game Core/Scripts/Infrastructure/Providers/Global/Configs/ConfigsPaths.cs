namespace GameCore.Infrastructure.Providers.Global
{
    public static class ConfigsPaths
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public const string InputReader = "Input Reader"; // TEMP
        
        public const string GameConfig = GlobalConfigs + "Game Config";
        public const string GameplayConfig = GlobalConfigs + "Gameplay Config";
        
        public const string ItemsListConfig = GameplayConfigs + "Items List Config";
        public const string LocationsListConfig = GameplayConfigs + "Locations List Config";
        public const string TimeConfig = GameplayConfigs + "Time Config";
        
        public const string PlayerConfig = Configs + "Player/Player Config";
        public const string GameBalanceConfig = Configs + "Game Balance Config";
        
        private const string GameData = "Game Data/";
        private const string Configs = GameData + "Configs/";
        private const string GlobalConfigs = Configs + "Global/";
        private const string GameplayConfigs = Configs + "Gameplay/";
    }
}