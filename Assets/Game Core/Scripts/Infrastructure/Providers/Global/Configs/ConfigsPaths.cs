namespace GameCore.Infrastructure.Providers.Global
{
    public static class ConfigsPaths
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public const string InputReader = "Input Reader"; // TEMP
        
        // Configs
        public const string GameConfig = GlobalConfigs + "Game";
        
        public const string BalanceConfig = GameplayConfigs + "Balance";
        public const string TimeConfig = GameplayConfigs + "Time";
        public const string TrainConfig = GameplayConfigs + "Train";
        public const string ElevatorConfig = GameplayConfigs + "Elevator";
        public const string QuestsConfig = GameplayConfigs + "Quests";
        public const string QuestsItemsConfig = GameplayConfigs + "Quests Items";
        public const string PlayerConfig = GameplayConfigs + "Player";
        public const string RigPresetsConfig = GameplayConfigs + "Rig Presets";
        public const string VisualConfig = GameplayConfigs + "Visual";
        public const string ItemsSpawnConfig = GameplayConfigs + "Items Spawn";
        public const string MonstersGeneratorConfig = GameplayConfigs + "Monsters Generator";
        // ----------
        
        // Configs Lists
        public const string EntitiesListConfig = GlobalConfigsLists + "Entities List";
        public const string MonstersListConfig = GlobalConfigsLists + "Monsters List";
        
        public const string ItemsListConfig = GameplayConfigsLists + "Items List";
        public const string LocationsListConfig = GameplayConfigsLists + "Locations List";
        public const string PrefabsListConfig = GameplayConfigsLists + "Prefabs List";
        // ----------
        
        // Monsters AI
        public const string GoodClownAIConfig = MonstersAIConfigs + "Good Clown AI";
        public const string EvilClownAIConfig = MonstersAIConfigs + "Evil Clown AI";
        public const string BeetleAIConfig = MonstersAIConfigs + "Beetle AI";
        public const string BlindCreatureAIConfig = MonstersAIConfigs + "Blind Creature AI";
        public const string SirenHeadAIConfig = MonstersAIConfigs + "Siren Head AI";
        // ----------
        
        // Entities
        public const string OutdoorChestConfig = EntitiesConfigs + "Outdoor Chest";
        // ----------
        
        // Other
        private const string GameData = "Game Data/";
        
        private const string Configs = GameData + "Configs/";
        private const string GlobalConfigs = Configs + "Global/";
        private const string GameplayConfigs = Configs + "Gameplay/";
        
        private const string ConfigsLists = GameData + "Configs Lists/";
        private const string GlobalConfigsLists = ConfigsLists + "Global/";
        private const string GameplayConfigsLists = ConfigsLists + "Gameplay/";
        
        private const string MonstersAIConfigs = GameData + "Monsters AI/";
        private const string EntitiesConfigs = GameData + "Entities Configs/";
        // ----------
    }
}