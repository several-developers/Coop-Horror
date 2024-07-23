namespace GameCore.Enums.Gameplay
{
    public enum GameState
    {
        HeadingToTheRoad = 1,
        ArrivedAtTheRoad = 2,
        HeadingToTheLocation = 4,
        ArrivedAtTheLocation = 5,
        ReadyToLeaveTheLocation = 6,
        KillPlayersOnTheRoad = 7,
        QuestsRewarding = 9,
        RestartGame = 10,
        //LeavingMainRoad = 11,
        EnteringMainRoad = 12,
        RestartGameCompleted = 13,
        
        
        GameOver = 0,
        WaitingForPlayers = 8,
        CycleMovement = 14,
        TeleportingToMarket = 15,
        TeleportingToMarketCompleted = 16,
        ArrivedAtMarketPlatform = 17,
        LeavingMarket = 18,
        LeavingMarketCompleted = 19,
        TeleportingToSector = 20,
        TeleportingToSectorCompleted = 21,
        ArrivedAtSectorPlatform = 22,
        LeavingSector = 23,
        LeavingSectorCompleted = 24,
    }
}