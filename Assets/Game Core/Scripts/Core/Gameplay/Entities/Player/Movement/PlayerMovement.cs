using System.Collections;
using GameCore.Core.Configs.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Ground Check")]
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private bool _isGround;

        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private float _groundAngle;
        
        [Header("Movement")]
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private bool _projectMoveOnGround;
        
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private Vector3 _moveInput;
        
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private Vector3 _moveVelocity;
        
        [Header("Slope and inertia")]
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private Vector3 _inertiaVelocity;

        [Header("Interaction with the platform")]
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private bool _platformAction;
        
        [BoxGroup(DebugInfo), SerializeField, ReadOnly]
        private Vector3 _platformVelocity;

        [Header("Slope and inertia")]
        [SerializeField]
        private float _slopeLimit = 45;

        [SerializeField]
        private float _inertiaDampingTime = 0.1f;
        
        [SerializeField]
        private float _slopeStartForce = 3f;
        
        [SerializeField]
        private float _slopeAcceleration = 3f;

        [Header("Collision")]
        [SerializeField]
        private bool _applyCollision = true;

        [SerializeField]
        private float _pushForce = 55f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerMovementConfigMeta _playerMovementConfig;

        // PROPERTIES: ----------------------------------------------------------------------------

        public CharacterController CharacterController { get; private set; }
        public Vector3 InertiaVelocity => _inertiaVelocity;
        public bool IsGround => _isGround;
        
        private Vector3 GroundNormal { get; set; }

        // FIELDS: --------------------------------------------------------------------------------

        private const string Platform = "Platform";
        private const string DebugInfo = "Debug Info";
        
        private IEnumerator _dampingCO;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() =>
            CharacterController = GetComponent<CharacterController>();

        private void Update()
        {
            GroundCheck();

            _moveVelocity = _projectMoveOnGround ? Vector3.ProjectOnPlane(_moveInput, GroundNormal) : _moveInput;

            if (_isGround)
            {
                if (_groundAngle < _slopeLimit && _inertiaVelocity != Vector3.zero)
                    InertiaDamping();
            }

            GravityUpdate();

            Vector3 moveDirection = (_moveVelocity + _inertiaVelocity + _platformVelocity);

            CharacterController.Move((moveDirection) * Time.deltaTime);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetMoveInput(Vector3 moveInput) =>
            _moveInput = moveInput;

        public void SetInertiaVelocity(Vector3 inertiaVelocity) =>
            _inertiaVelocity = inertiaVelocity;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void GravityUpdate()
        {
            if (_isGround && _groundAngle > _slopeLimit)
            {
                _inertiaVelocity +=
                    Vector3.ProjectOnPlane(
                        GroundNormal.normalized + (Vector3.down * (_groundAngle / 30)).normalized *
                        Mathf.Pow(_slopeStartForce, _slopeAcceleration), GroundNormal) * Time.deltaTime;
            }
            else if (!_isGround)
            {
                _inertiaVelocity.y -= Mathf.Pow(3f, 3) * Time.deltaTime;
            }
        }

        private void InertiaDamping()
        {
            Vector3 currentVelocity = Vector3.zero;

            //inertia braking when the force of movement is applied
            float resistanceAngle = Vector3.Angle(Vector3.ProjectOnPlane(_inertiaVelocity, GroundNormal),
                Vector3.ProjectOnPlane(_moveVelocity, GroundNormal));

            resistanceAngle = resistanceAngle == 0 ? 90 : resistanceAngle;

            _inertiaVelocity = (_inertiaVelocity + _moveVelocity).magnitude <= 0.1f
                ? Vector3.zero
                : Vector3.SmoothDamp(_inertiaVelocity, Vector3.zero, ref currentVelocity,
                    _inertiaDampingTime / (3 / (180 / resistanceAngle)));
        }

        private void GroundCheck()
        {
            if (Physics.SphereCast(transform.position, CharacterController.radius, Vector3.down, out RaycastHit hit,
                    CharacterController.height / 2 - CharacterController.radius + 0.01f))
            {
                _isGround = true;
                _groundAngle = Vector3.Angle(Vector3.up, hit.normal);
                GroundNormal = hit.normal;

                // if (hit.transform.CompareTag(Platform))
                // {
                //     _platformVelocity = hit.collider.attachedRigidbody == null | !_platformAction
                //         ? Vector3.zero
                //         : hit.collider.attachedRigidbody.velocity;
                // }

                if (Physics.BoxCast(transform.position,
                        new Vector3(CharacterController.radius / 2.5f, CharacterController.radius / 3f,
                            CharacterController.radius / 2.5f),
                        Vector3.down, out RaycastHit helpHit, transform.rotation,
                        CharacterController.height / 2 - CharacterController.radius / 2))
                {
                    _groundAngle = Vector3.Angle(Vector3.up, helpHit.normal);
                }
            }
            else
            {
                _platformVelocity = Vector3.zero;
                _isGround = false;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!_applyCollision)
                return;

            Rigidbody body = hit.collider.attachedRigidbody;

            // check rigidbody
            if (body == null || body.isKinematic)
                return;

            Vector3 pushDir = hit.point - (hit.point + hit.moveDirection.normalized);

            // Apply the push
            body.AddForce(pushDir * _pushForce, ForceMode.Force);
        }
    }
}