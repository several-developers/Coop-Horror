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
    }
}