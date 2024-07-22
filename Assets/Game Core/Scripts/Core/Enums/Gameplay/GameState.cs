namespace GameCore.Enums.Gameplay
{
    public enum GameState
    {
        GameOver = 0,
        HeadingToTheRoad = 1,
        ArrivedAtTheRoad = 2,
        ReadyToLeaveTheRoad = 3,
        HeadingToTheLocation = 4,
        ArrivedAtTheLocation = 5,
        ReadyToLeaveTheLocation = 6,
        KillPlayersOnTheRoad = 7,
        WaitingForPlayers = 8,
        QuestsRewarding = 9,
        RestartGame = 10,
        //LeavingMainRoad = 11,
        EnteringMainRoad = 12,
        RestartGameCompleted = 13,
    }
}