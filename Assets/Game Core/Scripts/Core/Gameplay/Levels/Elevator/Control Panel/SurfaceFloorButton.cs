namespace GameCore.Gameplay.Levels.Elevator
{
    public class SurfaceFloorButton : ControlPanelButton
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Interact()
        {
            ToggleInteract(canInteract: false);
            PlayAnimation();
        }
    }
}