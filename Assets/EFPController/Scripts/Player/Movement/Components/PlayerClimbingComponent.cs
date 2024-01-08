using EFPController.Extras;
using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public class PlayerClimbingComponent
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PlayerClimbingComponent(Player player, PlayerClimbingConfig climbingConfig)
        {
            _player = player;
            _playerMovement = player.controller;
            _inputManager = player.InputManager;
            _climbingConfig = climbingConfig;

            _rigidbody = player.Rigidbody;
            _transform = player.transform;
            _capsule = player.Capsule;
            _smoothLook = player.SmoothLook;
        }

        // PROPERTIES: ----------------------------------------------------------------------------
        
        // True when playing is in contact with ladder trigger or edge climbing trigger.
        public bool IsClimbing => ClimbingState != PlayerMovement.ClimbingState.None;
        
        public PlayerMovement.ClimbingState ClimbingState { get; private set; } = PlayerMovement.ClimbingState.None;

        // FIELDS: --------------------------------------------------------------------------------

        private readonly Player _player;
        private readonly PlayerMovement _playerMovement;
        private readonly InputManager _inputManager;
        private readonly PlayerClimbingConfig _climbingConfig;
        private readonly Rigidbody _rigidbody;
        private readonly Transform _transform;
        private readonly CapsuleCollider _capsule;
        private readonly SmoothLook _smoothLook;

        private float _climbingTargetVolume;
        private Ladder _activeLadder;
        private float _ladderPathPosition;
        private float _ladderPathTargetPosition;
        private Vector3 _ladderStartPosition;
        private Vector3 _ladderTargetPosition;
        private Quaternion _ladderStartRotation;
        private Quaternion _ladderTargetRotation;
        private float _ladderTime;
        private bool _climbInteract;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ClimbingUpdateLogic()
        {
            if (_climbInteract && !_inputManager.GetActionKey(InputManager.Action.Jump))
                _climbInteract = false;

            if (IsClimbing && ClimbingState != PlayerMovement.ClimbingState.Grabbed)
                Climbing();
        }

        public void Climbing()
        {
            AudioSource climbingAudioSource = _playerMovement._climbingAudioSource;
            float grabbingTime = _climbingConfig.grabbingTime;
            float climbingSpeed = _climbingConfig.climbingSpeed;
            bool rotateClimbingCamera = _climbingConfig.rotateClimbingCamera;

            switch (ClimbingState)
            {
                case PlayerMovement.ClimbingState.Releasing:
                    _ladderTime += Time.deltaTime;
                    if (_ladderTime < grabbingTime)
                    {
                        _transform.position = Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition,
                            _ladderTime / grabbingTime);
                    }
                    else
                    {
                        _rigidbody.isKinematic = false;
                        _ladderTime = 0f;
                        ClimbingState = PlayerMovement.ClimbingState.None;
                        UnClimb();
                    }

                    break;
                
                case PlayerMovement.ClimbingState.Grabbing:
                    _ladderTime += Time.deltaTime;
                    if (_ladderTime < grabbingTime)
                    {
                        if (rotateClimbingCamera)
                        {
                            _smoothLook.transform.rotation = Quaternion.Lerp(_ladderStartRotation,
                                _ladderTargetRotation, _ladderTime / grabbingTime);
                        }

                        _transform.position = Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition,
                            _ladderTime / grabbingTime);
                    }
                    else
                    {
                        _rigidbody.isKinematic = false;

                        if (rotateClimbingCamera)
                        {
                            _player.Rotate(_ladderTargetRotation);
                            _smoothLook.enabled = true;
                        }

                        _ladderTime = 0f;
                        ClimbingState = PlayerMovement.ClimbingState.Grabbed;

                        if (climbingAudioSource != null && _activeLadder.climbingSound != null)
                        {
                            climbingAudioSource.clip = _activeLadder.climbingSound;
                            climbingAudioSource.loop = true;
                            climbingAudioSource.Play();
                            climbingAudioSource.Pause();
                            _climbingTargetVolume = 0f;
                            climbingAudioSource.volume = 0f;
                        }
                    }

                    break;

                case PlayerMovement.ClimbingState.Grabbed:
                    // Get the path position from character's current position
                    Vector3 p1 =
                        _activeLadder.ClosestPointOnPath(_playerMovement.PlayerBottomPos, out _ladderPathPosition);
                    Transform closestPoint = _activeLadder.GetClosestPointByPosition(_playerMovement.PlayerBottomPos);
                    Vector3 p2 = _activeLadder.ClosestPointOnPath(closestPoint.position, out _ladderPathTargetPosition);
                    float dist = Vector3.Distance(p1, p2);
                    if (dist > 0.2f)
                    {
                        // Move the character along the ladder path
                        _rigidbody.velocity = _activeLadder.transform.up * (_playerMovement.InputY * climbingSpeed * Time.fixedDeltaTime);

                        if (climbingAudioSource != null && _activeLadder.climbingSound != null)
                        {
                            if (_playerMovement.InputY.IsZero())
                            {
                                _climbingTargetVolume = 0f;
                            }
                            else
                            {
                                _climbingTargetVolume = 1f;
                            }

                            climbingAudioSource.volume = Mathf.Lerp(climbingAudioSource.volume, _climbingTargetVolume,
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
                        _ladderTime = 0f;
                        ClimbingState = PlayerMovement.ClimbingState.Releasing;
                        Transform currPoint = _activeLadder.GetClosestPointByPosition(_playerMovement.PlayerBottomPos);
                        _ladderStartPosition = _transform.position;
                        _ladderTargetPosition =
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

            if (ClimbingState == PlayerMovement.ClimbingState.Grabbed)
            {
                if (!_climbInteract && (_inputManager.GetActionKey(InputManager.Action.Interact) ||
                                                          _inputManager.GetActionKey(InputManager.Action.Jump)))
                {
                    _climbInteract = true;
                    StopClimbing();
                }
            }
            else if (!IsClimbing && _climbingConfig.climbOnInteract && !_climbInteract)
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
                {
                }

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
            Vector3 playerBottomPos = _playerMovement.PlayerBottomPos;

            _ladderTime = 0f;
            _activeLadder = ladder;
            ClimbingState = PlayerMovement.ClimbingState.Grabbing;

            Transform closestPoint = ladder.GetClosestPointByPosition(playerBottomPos);
            Vector3 dir = ladder.transform.up;
            if (closestPoint == ladder.topPoint) dir = -ladder.transform.up;

            _ladderStartPosition = _transform.position;
            _ladderTargetPosition =
                (_activeLadder.ClosestPointOnPath(playerBottomPos, out _ladderPathPosition) + dir * 0.4f) +
                _transform.up * (_capsule.height * 0.5f);

            _ladderStartRotation = _smoothLook.transform.rotation;
            _ladderTargetRotation = closestPoint.rotation;

            _rigidbody.useGravity = false;

            if (clampClimbingCameraRotate)
                _smoothLook.maxXAngle = clampClimbingCameraAngle;

            if (rotateClimbingCamera)
                _smoothLook.enabled = false;

            _playerMovement.JumpingComponent.IsJumping = false;
            _playerMovement.IsFalling = false;
            _playerMovement.SprintComponent.Sprint = _playerMovement.SprintComponent.SprintActive = false;
            _playerMovement.SwimmingComponent.IsSwimming = false;

            if (_player.CameraAnimator.HasParameter(CameraAnimNames.Climb))
                _player.CameraAnimator.SetTrigger(CameraAnimNames.Climb);

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
                _smoothLook.maxXAngle = _smoothLook.MaxXAngleDef;

            if (rotateClimbingCamera)
                _smoothLook.enabled = true;

            if (_playerMovement._climbingAudioSource != null && _activeLadder.climbingSound != null)
            {
                _playerMovement.SmoothVolume(_playerMovement._climbingAudioSource, 0f, 0.5f,
                    () => _playerMovement._climbingAudioSource.Stop());
            }
        }
        
        private void StopClimbing()
        {
            if (ClimbingState != PlayerMovement.ClimbingState.Grabbed)
                return;

            ClimbingState = PlayerMovement.ClimbingState.None;
            UnClimb();
        }
    }
}