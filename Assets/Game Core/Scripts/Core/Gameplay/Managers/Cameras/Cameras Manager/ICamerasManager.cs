using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player.CameraManagement;

namespace GameCore.Gameplay.Managers.Cameras
{
    public interface ICamerasManager
    {
        void SetCameraStatus(CameraStatus cameraStatus);
        void SwitchToNextPlayer();
        PlayerCamera GetPlayerCamera();
    }
}