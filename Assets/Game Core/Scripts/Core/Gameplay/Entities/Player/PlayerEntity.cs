using System;
using DG.Tweening;
using GameCore.Gameplay.Entities.Player.Movement;
using GameCore.Gameplay.Entities.Player.Rotation;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Observers.Taps;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GameCore.Gameplay.Entities.Player
{
    public class PlayerEntity : MonoBehaviour, IPlayerEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _aimMoveSpeed = 1000f;

        [SerializeField, DisableInPlayMode]
        private bool _useJoystickRotation;

        [SerializeField]
        private bool _isDead;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private Rig _aimRig;

        [SerializeField, Required]
        private MultiAimConstraint _headAimConstraint;

        [SerializeField, Required]
        private MultiAimConstraint _weaponAimConstraint;

        [SerializeField, Required]
        private TwoBoneIKConstraint _secondHandIK;

        [SerializeField, Required]
        private Transform _cinemachineCameraTarget;

        [SerializeField, Required]
        private Transform _lookAtTransform;

        [SerializeField, Required]
        private Transform _aimTarget;

        [SerializeField, Required]
        private Transform _shootPoint;

        [SerializeField, Required]
        private LineRenderer _laserLR;

        [SerializeField, Required]
        private ParticleSystem _muzzleFlashPS;

        [SerializeField, Required]
        private ParticleSystem _bloodImpactPS;

        [SerializeField]
        private LayerMask _aimLayer;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action<Vector2> OnMovementVectorChangedEvent;

        private Camera _camera;
        private Transform _cameraTransform;
        private IMovementBehaviour _movementBehaviour;
        private IRotationBehaviour _rotationBehaviour;
        private TargetsSearcher _targetsSearcher;
        private Vector3 _centerOfScreen;
        private ITapsObserver _tapsObserver;
        private bool _canShoot;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            var inputSystemListener = GetComponent<InputSystemListener>();
            inputSystemListener.OnMoveEvent += OnMove;
            inputSystemListener.OnLookEvent += OnLook;
            inputSystemListener.OnChangeCursorStateEvent += OnChangeCursorState;
            inputSystemListener.OnChangeCameraLockStateEvent += OnChangeCameraLockState;

            _centerOfScreen = new Vector2(0.5f, 0.5f);

            if (!_useJoystickRotation)
                ChangeCursorState();
        }

        private void Update()
        {
            _movementBehaviour?.Movement();
            //TryShoot();

            if (Input.GetKeyDown(KeyCode.R))
                ReloadWeapon();
        }

        private void LateUpdate()
        {
            MoveAimTarget();
            _rotationBehaviour?.Rotate();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Setup(LevelManager levelManager, Camera mainCamera, ITapsObserver tapsObserver)
        {
            _camera = mainCamera;
            _tapsObserver = tapsObserver;
            _cameraTransform = mainCamera.transform;
            _targetsSearcher = new TargetsSearcher(_camera);
            
            _tapsObserver.OnTapDownEvent += OnTapDownEvent;
            _tapsObserver.OnTapUpEvent += OnTapUp;

            float reloadTime = 1.5f;
            float reloadAnimationTime = _animator.GetAnimationTime(AnimatorHashes.ReloadingAnimation);
            float reloadTimeMultiplier = reloadAnimationTime / reloadTime; // 1, 0.5, 1 / 0.5

            _animator.SetFloat(id: AnimatorHashes.ReloadMultiplier, reloadTimeMultiplier);

            var controller = GetComponent<CharacterController>();

            _movementBehaviour = new FreeMovementBehaviour(playerEntity: this, controller, _camera);
            _rotationBehaviour = new FreeRotationBehaviour(playerEntity: this, _cinemachineCameraTarget);
        }

        public void TakeDamage(IEntity source, float damage) =>
            _animator.SetTrigger(id: AnimatorHashes.HitReaction);

        public void ReloadWeapon()
        {
        }

        public Transform GetTransform() => transform;

        public Transform GetLookAtTransform() => _lookAtTransform;

        public Animator GetAnimator() => _animator;

        public bool IsDead() => _isDead;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MoveAimTarget()
        {
            Ray ray;

            if (_useJoystickRotation)
                ray = _camera.ViewportPointToRay(_centerOfScreen);
            else
                ray = _camera.ScreenPointToRay(Input.mousePosition);

            Vector3 playerPosition = transform.position;
            Vector3 cameraPosition = _cameraTransform.position;
            float distanceFromCameraToPlayer = Vector3.Distance(playerPosition, cameraPosition);

            Vector3 viewportToWorldPoint = _camera.ViewportToWorldPoint(_centerOfScreen);
            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 origin = viewportToWorldPoint + cameraForward * (distanceFromCameraToPlayer + 0.1f);

            bool isHitSomething = Physics.Raycast(origin, ray.direction, out RaycastHit raycastHit,
                maxDistance: float.MaxValue, _aimLayer);

            if (!isHitSomething)
                return;

            //_aimTarget.position = raycastHit.point;
            _aimTarget.position = Vector3.MoveTowards(current: _aimTarget.position, target: raycastHit.point,
                maxDistanceDelta: _aimMoveSpeed * Time.deltaTime);

            _laserLR.SetPosition(index: 0, _shootPoint.position);
            _laserLR.SetPosition(index: 1, raycastHit.point);
        }

        private void TryShoot()
        {
            bool isTargetFound = _targetsSearcher.FindTarget();

            if (!isTargetFound)
                return;

            if (!_canShoot)
                return;

            _canShoot = false;
        }

        private static void ChangeCursorState()
        {
            CursorLockMode lockState = Cursor.lockState;

            Cursor.lockState = lockState switch
            {
                CursorLockMode.None => CursorLockMode.Locked,
                CursorLockMode.Locked => CursorLockMode.None,
                _ => Cursor.lockState
            };
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 movementVector) =>
            OnMovementVectorChangedEvent?.Invoke(movementVector);

        private void OnLook(Vector2 lookVector)
        {
            if (_useJoystickRotation)
                return;

            _rotationBehaviour?.SetLookVector(lookVector);
        }

        private void OnChangeCameraLockState() =>
            _rotationBehaviour?.ChangeLockCameraPosition();

        private void OnShoot()
        {
            _animator.SetTrigger(AnimatorHashes.Attack);
            _muzzleFlashPS.Play();
        }

        private void OnHit(RaycastHit hitInfo)
        {
            Vector3 hitPosition = hitInfo.point;
            Vector3 direction = (hitPosition - transform.position).normalized;

            ParticleSystem bloodImpactPS = Instantiate(_bloodImpactPS, hitPosition, Quaternion.identity);
            bloodImpactPS.transform.LookAt(transform);

            Destroy(bloodImpactPS.gameObject, 10f);
        }

        private void OnReloadingStarted()
        {
            DOVirtual.Float(from: 1f, to: 0f, duration: 0.5f, OnVirtualUpdate);
            _animator.SetTrigger(AnimatorHashes.Reload);

            // LOCAL METHODS: -----------------------------

            void OnVirtualUpdate(float t)
            {
                _secondHandIK.weight = t;
                _headAimConstraint.weight = t;
                _weaponAimConstraint.weight = t;
            }
        }

        private void OnReloadingFinished()
        {
            DOVirtual.Float(from: 0f, to: 1f, duration: 0.5f, OnVirtualUpdate);

            // LOCAL METHODS: -----------------------------

            void OnVirtualUpdate(float t)
            {
                _secondHandIK.weight = t;
                _headAimConstraint.weight = t;
                _weaponAimConstraint.weight = t;
            }
        }

        private void OnTapDownEvent() =>
            _canShoot = false;

        private void OnTapUp()
        {
            _canShoot = true;
        }

        private static void OnChangeCursorState() => ChangeCursorState();
    }
}