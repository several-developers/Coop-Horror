using EFPController.Extras;
using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public class PlayerClimbingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerClimbingComponent(Player player, PlayerMovement playerMovement, InputManager inputManager,
            PlayerClimbingConfig climbingConfig)
        {
            _player = player;
            _playerMovement = playerMovement;
            _inputManager = inputManager;
            _climbingConfig = climbingConfig;

            _rigidbody = player.rigidbody;
            _transform = player.transform;
            _capsule = player.capsule;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerClimbingConfig ClimbingConfig => _climbingConfig;

        // True when playing is in contact with ladder trigger or edge climbing trigger.
        public bool IsClimbing => climbingState != PlayerMovement.ClimbingState.None;

        public bool ClimbInteract { get; set; }

        public PlayerMovement.ClimbingState climbingState { get; set; } = PlayerMovement.ClimbingState.None;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly InputManager _inputManager;
        private readonly PlayerClimbingConfig _climbingConfig;
        private readonly Rigidbody _rigidbody;
        private readonly Transform _transform;
        private readonly CapsuleCollider _capsule;

        private float climbingTargetVolume;
        private Ladder activeLadder;
        private float ladderPathPosition;
        private float ladderPathTargetPosition;
        private Vector3 ladderStartPosition;
        private Vector3 ladderTargetPosition;
        private Quaternion ladderStartRotation;
        private Quaternion ladderTargetRotation;
        private float ladderTime;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ClimbingUpdateLogic()
        {
            if (ClimbInteract && !_inputManager.GetActionKey(InputManager.Action.Jump))
                ClimbInteract = false;

            if (IsClimbing && climbingState != PlayerMovement.ClimbingState.Grabbed)
                Climbing();
        }

        public void Climbing()
        {
            AudioSource climbingAudioSource = _playerMovement._climbingAudioSource;
            float grabbingTime = _climbingConfig.grabbingTime;
            float climbingSpeed = _climbingConfig.climbingSpeed;
            bool rotateClimbingCamera = _climbingConfig.rotateClimbingCamera;

            switch (climbingState)
            {
                case PlayerMovement.ClimbingState.Releasing:
                    ladderTime += Time.deltaTime;
                    if (ladderTime < grabbingTime)
                    {
                        _transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition,
                            ladderTime / grabbingTime);
                    }
                    else
                    {
                        _rigidbody.isKinematic = false;
                        ladderTime = 0f;
                        climbingState = PlayerMovement.ClimbingState.None;
                        UnClimb();
                    }

                    break;
                case PlayerMovement.ClimbingState.Grabbing:
                    ladderTime += Time.deltaTime;
                    if (ladderTime < grabbingTime)
                    {
                        if (rotateClimbingCamera)
                        {
                            _player.smoothLook.transform.rotation = Quaternion.Lerp(ladderStartRotation,
                                ladderTargetRotation, ladderTime / grabbingTime);
                        }

                        _transform.position = Vector3.Lerp(ladderStartPosition, ladderTargetPosition,
                            ladderTime / grabbingTime);
                    }
                    else
                    {
                        _rigidbody.isKinematic = false;

                        if (rotateClimbingCamera)
                        {
                            _player.Rotate(ladderTargetRotation);
                            _player.smoothLook.enabled = true;
                        }

                        ladderTime = 0f;
                        climbingState = PlayerMovement.ClimbingState.Grabbed;

                        if (climbingAudioSource != null && activeLadder.climbingSound != null)
                        {
                            climbingAudioSource.clip = activeLadder.climbingSound;
                            climbingAudioSource.loop = true;
                            climbingAudioSource.Play();
                            climbingAudioSource.Pause();
                            climbingTargetVolume = 0f;
                            climbingAudioSource.volume = 0f;
                        }
                    }

                    break;

                case PlayerMovement.ClimbingState.Grabbed:
                    // Get the path position from character's current position
                    Vector3 p1 =
                        activeLadder.ClosestPointOnPath(_playerMovement.playerBottomPos, out ladderPathPosition);
                    Transform closestPoint = activeLadder.GetClosestPointByPosition(_playerMovement.playerBottomPos);
                    Vector3 p2 = activeLadder.ClosestPointOnPath(closestPoint.position, out ladderPathTargetPosition);
                    float dist = Vector3.Distance(p1, p2);
                    if (dist > 0.2f)
                    {
                        // Move the character along the ladder path
                        _rigidbody.velocity = activeLadder.transform.up * _playerMovement.inputY * climbingSpeed *
                                              Time.fixedDeltaTime;

                        if (climbingAudioSource != null && activeLadder.climbingSound != null)
                        {
                            if (_playerMovement.inputY.IsZero())
                            {
                                climbingTargetVolume = 0f;
                            }
                            else
                            {
                                climbingTargetVolume = 1f;
                            }

                            climbingAudioSource.volume = Mathf.Lerp(climbingAudioSource.volume, climbingTargetVolume,
                                10f * Time.deltaTime);
                            if (climbingAudioSource.volume.IsZero())
                            {
                                climbingAudioSource.Pause();
                            }
                            else
                            {
                                climbingAudioSource.UnPause();
                            }
                        }
                    }
                    else
                    {
                        // If reached on of the ladder path extremes, change to releasing phase
                        ladderTime = 0f;
                        climbingState = PlayerMovement.ClimbingState.Releasing;
                        Transform currPoint = activeLadder.GetClosestPointByPosition(_playerMovement.playerBottomPos);
                        ladderStartPosition = _transform.position;
                        ladderTargetPosition =
                            currPoint.position + _transform.up * (_playerMovement.capsule.height * 0.5f);
                    }

                    break;
            }
        }

        public bool TriggerEnterLogic(Collider other)
        {
            bool canClimb = _climbingConfig.allowClimbing &&
                            !IsClimbing &&
                            !_climbingConfig.climbOnInteract &&
                            other.gameObject.CompareTag(Game.Tags.Climbable);

            if (!canClimb)
                return false;
            
            Ladder ladder = other.gameObject.GetComponent<Ladder>();

            if (ladder != null)
                TryClimbing(ladder);

            return true;
        }

        public void TriggerStayLogic(Collider other)
        {
            bool canClimb = other.gameObject.CompareTag(Game.Tags.Climbable);

            if (!canClimb)
                return;

            if (climbingState == PlayerMovement.ClimbingState.Grabbed)
            {
                if (!ClimbInteract && (_inputManager.GetActionKey(InputManager.Action.Interact) ||
                                                          _inputManager.GetActionKey(InputManager.Action.Jump)))
                {
                    ClimbInteract = true;
                    StopClimbing();
                }
            }
            else if (!IsClimbing && _climbingConfig.climbOnInteract && !ClimbInteract)
            {
                Ladder ladder = other.gameObject.GetComponent<Ladder>();
                    
                if (ladder != null)
                    TryClimbing(ladder);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TryClimbing(Ladder ladder)
        {
            float angleToClimb = _climbingConfig.angleToClimb;
            bool climbOnInteract = _climbingConfig.climbOnInteract;

            Vector3 ladderPos = ladder.centerPoint.position;
            ladderPos.y = 0f;
            Vector3 playerPos = _transform.position;
            playerPos.y = 0f;
            Vector3 dirToLadder = (ladderPos - playerPos).normalized;
            Vector3 forwardPlayer = _transform.forward;
            float angle = Vector3.Angle(forwardPlayer, dirToLadder);

            if (angle < angleToClimb)
            {
                if (climbOnInteract)
                    _playerMovement.hoverClimbable = true;

                if (!climbOnInteract || _inputManager.GetActionKey(InputManager.Action.Interact))
                    _playerMovement.ClimbingComponent.Climb(ladder);
            }
        }
        
        private void Climb(Ladder ladder)
        {
            if (IsClimbing)
                return;

            float clampClimbingCameraAngle = _climbingConfig.clampClimbingCameraAngle;
            bool clampClimbingCameraRotate = _climbingConfig.clampClimbingCameraRotate;
            bool rotateClimbingCamera = _climbingConfig.rotateClimbingCamera;
            Vector3 playerBottomPos = _playerMovement.playerBottomPos;

            ladderTime = 0f;
            activeLadder = ladder;
            climbingState = PlayerMovement.ClimbingState.Grabbing;

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

            if (rotateClimbingCamera)
                _player.smoothLook.enabled = false;

            _playerMovement.JumpingComponent.IsJumping = false;
            _playerMovement.falling = false;
            _playerMovement.sprint = _playerMovement.sprintActive = false;
            _playerMovement.IsSwimming = false;

            if (_player.cameraAnimator.HasParameter(CameraAnimNames.Climb))
                _player.cameraAnimator.SetTrigger(CameraAnimNames.Climb);

            if (_playerMovement._climbingAudioSource != null && ladder.climbSound != null)
            {
                _playerMovement._climbingAudioSource.volume = 1f;
                _playerMovement._climbingAudioSource.PlayOneShot(ladder.climbSound, Random.Range(0.5f, 1f));
            }

            _playerMovement.Stop();
        }

        private void UnClimb()
        {
            bool clampClimbingCameraRotate = _climbingConfig.clampClimbingCameraRotate;
            bool rotateClimbingCamera = _climbingConfig.rotateClimbingCamera;

            _rigidbody.useGravity = true;

            if (clampClimbingCameraRotate)
                _player.smoothLook.maxXAngle = _player.smoothLook.maxXAngleDef;

            if (rotateClimbingCamera)
                _player.smoothLook.enabled = true;

            if (_playerMovement._climbingAudioSource != null && activeLadder.climbingSound != null)
            {
                _playerMovement.SmoothVolume(_playerMovement._climbingAudioSource, 0f, 0.5f,
                    () => _playerMovement._climbingAudioSource.Stop());
            }
        }
        
        private void StopClimbing()
        {
            if (climbingState != PlayerMovement.ClimbingState.Grabbed)
                return;

            climbingState = PlayerMovement.ClimbingState.None;
            UnClimb();
        }
    }
}