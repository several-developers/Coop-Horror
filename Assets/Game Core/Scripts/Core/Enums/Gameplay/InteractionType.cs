namespace GameCore.Enums.Gameplay
{
    public enum InteractionType
    {
        None = 0,
        PickUpItem = 1,
        Open = 2,
        Close = 3,
        ToggleMobileDoor = 4, // DEPRECATED
        MobileHQMainLever = 5,
        LeaveLocationMobileLever = 6, // DEPRECATED
        ElevatorFloorButton = 7,
        ElevatorCallButton = 8,
        FireExitDoor = 9,
        SimpleButton = 10,
        Heliport = 11,
        MobileHQSeat = 12,
    }
}