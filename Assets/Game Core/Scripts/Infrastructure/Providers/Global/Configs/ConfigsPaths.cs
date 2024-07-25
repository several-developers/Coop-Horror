namespace GameCore.Infrastructure.Providers.Global
{
    public static class ConfigsPaths
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public const string InputReader = "Input Reader"; // TEMP
        
        public const string GameConfig = GlobalConfigs + "Game Config";
        
        public const string BalanceConfig = GameplayConfigs + "Balance Config";
        public const string ItemsListConfig = GameplayConfigs + "Items List Config";
        public const string LocationsListConfig = GameplayConfigs + "Locations List Config";
        public const string TimeConfig = GameplayConfigs + "Time Config";
        public const string TrainConfig = GameplayConfigs + "Train Config";
        public const string DungeonGeneratorConfig = GameplayConfigs + "Dungeon Generator Config";
        public const string ElevatorConfig = GameplayConfigs + "Elevator Config";
        public const string PrefabsListConfig = GameplayConfigs + "Prefabs List Config";
        public const string QuestsConfig = GameplayConfigs + "Quests Config";
        public const string QuestsItemsConfig = GameplayConfigs + "Quests Items Config";
        public const string PlayerConfig = GameplayConfigs + "Player Config";
        public const string RigPresetsConfig = GameplayConfigs + "Rig Presets Config";
        public const string VisualConfig = GameplayConfigs + "Visual Config";
        public const string ItemsSpawnConfig = GameplayConfigs + "Items Spawn Config";
        public const string MonstersListConfig = GameplayConfigs + "Monsters List Config";

        public const string GoodClownAIConfig = MonstersAIConfigs + "Good Clown AI";
        public const string EvilClownAIConfig = MonstersAIConfigs + "Evil Clown AI";
        public const string BeetleAIConfig = MonstersAIConfigs + "Beetle AI";
        
        public const string GameBalanceConfig = Configs + "Game Balance Config";
        
        private const string GameData = "Game Data/";
        private const string Configs = GameData + "Configs/";
        private const string GlobalConfigs = Configs + "Global/";
        private const string GameplayConfigs = Configs + "Gameplay/";
        
        private const string MonstersAIConfigs = GameData + "Monsters AI/";
    }
}