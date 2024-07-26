using System.Collections.Generic;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Train;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Entities.Player.CameraManagement;
using UnityEngine;

namespace GameCore.Gameplay.CamerasManagement
{
    public class CamerasManager : ICamerasManager
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CamerasManager(ITrainEntity trainEntity, PlayerCamera playerCamera,
            SpectatorCamera spectatorCamera)
        {
            _trainEntity = trainEntity;
            _playerCamera = playerCamera;
            _spectatorCamera = spectatorCamera;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly ITrainEntity _trainEntity;
        private readonly PlayerCamera _playerCamera;
        private readonly SpectatorCamera _spectatorCamera;

        private CameraStatus _cameraStatus;
        private int _lastSpectateIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetCameraStatus(CameraStatus cameraStatus)
        {
            switch (cameraStatus)
            {
                case CameraStatus.Spectator:
                {
                    if (IsAnyAlivePlayersLeft())
                        SwitchToNextPlayer();
                    else
                        cameraStatus = CameraStatus.OutsideTrain;

                    break;
                }
            }

            _cameraStatus = cameraStatus;
            CheckCamerasState(cameraStatus);
        }

        public void SwitchToNextPlayer()
        {
            bool isCameraStatusValid = _cameraStatus != CameraStatus.Spectator;

            if (!isCameraStatusValid)
                return;

            IReadOnlyDictionary<ulong, PlayerEntity> allPlayers = PlayerEntity.GetAllPlayers();
            List<ulong> clientsID = new(allPlayers.Keys);
            bool alivePlayerFound = false;

            int iterations = 0;

            while (!alivePlayerFound)
            {
                if (iterations > 50)
                {
                    Debug.LogError("INFINITE LOOP!");
                    break;
                }

                iterations++;

                if (_lastSpectateIndex >= clientsID.Count)
                    _lastSpectateIndex = 0;

                ulong clientID = clientsID[_lastSpectateIndex];
                bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);
                bool isPlayerValid = isPlayerFound && !playerEntity.IsDead();

                if (!isPlayerValid)
                {
                    _lastSpectateIndex++;
                    continue;
                }

                alivePlayerFound = true;
                _spectatorCamera.UpdateTarget(playerEntity);
            }

            if (!alivePlayerFound)
                Debug.LogWarning("FAILED");
        }

        public PlayerCamera GetPlayerCamera() => _playerCamera;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckCamerasState(CameraStatus cameraStatus)
        {
            CheckMobileHQCamera(cameraStatus);
            CheckPlayerCamera(cameraStatus);
            CheckSpectatorCamera(cameraStatus);
        }

        private void CheckMobileHQCamera(CameraStatus cameraStatus)
        {
            bool isEnabled = cameraStatus == CameraStatus.OutsideTrain;
            Camera camera = _trainEntity.GetOutsideCamera();
            camera.enabled = isEnabled;
        }

        private void CheckPlayerCamera(CameraStatus cameraStatus)
        {
            bool isEnabled = cameraStatus == CameraStatus.FirstPerson;
            _playerCamera.ToggleCameraState(isEnabled);
        }

        private void CheckSpectatorCamera(CameraStatus cameraStatus)
        {
            bool isEnabled = cameraStatus == CameraStatus.Spectator;
            _spectatorCamera.ToggleCameraState(isEnabled);

            // TEMP
            PlayerEntity localPlayer = PlayerEntity.GetLocalPlayer();
            PlayerReferences playerReferences = localPlayer.References;
            playerReferences.PlayerMovementController.ToggleCameraState(!isEnabled);
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