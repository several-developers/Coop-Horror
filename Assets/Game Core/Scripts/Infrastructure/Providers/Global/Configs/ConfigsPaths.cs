﻿namespace GameCore.Infrastructure.Providers.Global
{
    public static class ConfigsPaths
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public const string GameConfig = GlobalConfigs + "Game Config";
        public const string GameplayConfig = GlobalConfigs + "Gameplay Config";
        public const string PlayerConfig = Configs + "Player/Player Config";
        public const string GameBalanceConfig = Configs + "Game Balance Config";
        public const string LevelsConfig = GlobalConfigs + "Levels Config";
        
        private const string GameData = "Game Data/";
        private const string Configs = GameData + "Configs/";
        private const string GlobalConfigs = Configs + "Global/";
    }
}