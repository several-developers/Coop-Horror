namespace GameCore.Gameplay.Entities.Player.Movement
{
    public interface IMovementBehaviour
    {
        void Tick();
        void FixedTick();
        void Dispose();
        void ToggleState(bool isEnabled);
    }
}