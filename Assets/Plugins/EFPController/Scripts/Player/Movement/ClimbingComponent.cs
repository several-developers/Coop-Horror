using EFPController.Extras;
using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public class ClimbingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClimbingComponent(Player player, PlayerMovement playerMovement)
        {
            _player = player;
            _playerMovement = playerMovement;

            _rigidbody = player.rigidbody;
            _transform = player.transform;
            _capsule = player.capsule;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly Rigidbody _rigidbody;
        private readonly Transform _transform;
        private readonly CapsuleCollider _capsule;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        /*public void Climb(Ladder ladder)
        {
            if (_playerMovement.climbing)
                return;
            
            ladderTime = 0f;
            activeLadder = ladder;
            _playerMovement.climbingState = PlayerMovement.ClimbingState.Grabbing;

            Transform closestPoint = ladder.GetClosestPointByPosition(playerBottomPos);
            Vector3 dir = ladder.transform.up;
            if (closestPoint == ladder.topPoint) dir = -ladder.transform.up;

            ladderStartPosition = _transform.position;
            ladderTargetPosition =
                (activeLadder.ClosestPointOnPath(playerBottomPos, out ladderPathPosition) + dir * 0.4f) +
                _transform.up * (_capsule.height * 0.5f);

            ladderStartRotation = _player.smoothLook.transform.rotation;
            ladderTargetRotation = closestPoint.rotation;

            _rigidbody.useGravity = false;
            
            if (clampClimbingCameraRotate)
                _player.smoothLook.maxXAngle = clampClimbingCameraAngle;
            
            if (rotateClibbingCamera)
                _player.smoothLook.enabled = false;

            _playerMovement.JumpingComponent.IsJumping = false;
            _playerMovement.falling = false;
            _playerMovement.sprint = sprintActive = false;
            IsSwimming = false;

            if (_player.cameraAnimator.HasParameter(CameraAnimNames.Climb))
                _player.cameraAnimator.SetTrigger(CameraAnimNames.Climb);

            if (_playerMovement.climbingAudioSource != null && ladder.climbSound != null)
            {
                _playerMovement.climbingAudioSource.volume = 1f;
                _playerMovement.climbingAudioSource.PlayOneShot(ladder.climbSound, Random.Range(0.5f, 1f));
            }

            _playerMovement.Stop();
        }*/
    }
}