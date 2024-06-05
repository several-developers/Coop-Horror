using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.MobileHeadquarters;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using UnityEngine;

namespace GameCore.Gameplay.CamerasManagement
{
    public class CamerasManager : ICamerasManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CamerasManager(IMobileHeadquartersEntity mobileHeadquartersEntity, PlayerCamera playerCamera)
        {
            _mobileHeadquartersEntity = mobileHeadquartersEntity;
            _playerCamera = playerCamera;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly IMobileHeadquartersEntity _mobileHeadquartersEntity;
        private readonly PlayerCamera _playerCamera;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetCameraStatus(CameraStatus cameraStatus)
        {
            switch (cameraStatus)
            {
                case CameraStatus.Spectator:
                    if (!IsAnyAlivePlayersLeft())
                        cameraStatus = CameraStatus.OutsideMobileHQ;
                    break;
            }
            
            HandleCameraStatus(cameraStatus);
        }

        public PlayerCamera GetPlayerCamera() => _playerCamera;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void HandleCameraStatus(CameraStatus cameraStatus)
        {
            CheckMobileHQ(cameraStatus);
            CheckPlayer(cameraStatus);
        }
        
        private void CheckMobileHQ(CameraStatus cameraStatus)
        {
            bool enable = cameraStatus == CameraStatus.OutsideMobileHQ;
            Camera camera = _mobileHeadquartersEntity.GetOutsideCamera();
            camera.enabled = enable;
        }

        private void CheckPlayer(CameraStatus cameraStatus)
        {
            bool enable = cameraStatus == CameraStatus.FirstPerson;
            Camera camera = _playerCamera.CameraReferences.MainCamera;
            camera.enabled = enable;
        }

        private static bool IsAnyAlivePlayersLeft()
        {
            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            bool isAnyAlivePlayersLeft = false;

            foreach (PlayerEntity playerEntity in allPlayers.Values)
            {
                bool isDead = playerEntity.IsDead();

                if (isDead)
                    continue;

                isAnyAlivePlayersLeft = true;
                break;
            }

            return isAnyAlivePlayersLeft;
        }
    }
}