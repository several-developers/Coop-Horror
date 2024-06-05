using GameCore.Enums.Gameplay;
using GameCore.Gameplay.CamerasManagement;

namespace GameCore.Gameplay.Entities.Player.States
{
    public class AliveState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public AliveState(PlayerEntity playerEntity, ICamerasManager camerasManager)
        {
            _playerEntity = playerEntity;
            _camerasManager = camerasManager;
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly PlayerEntity _playerEntity;
        private readonly ICamerasManager _camerasManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter() => SetCameraFirstPersonStatus();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetCameraFirstPersonStatus() =>
            _camerasManager.SetCameraStatus(CameraStatus.FirstPerson);
    }
}