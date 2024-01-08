using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using EFPController.Utils;
using EFPController.Extras;

namespace EFPController
{
    public class Player : MonoBehaviour
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // static
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool canControl { get; private set; }
        public static bool canCameraControl { get; private set; }
        public static bool canInteractable { get; private set; }
        public static Player instance;
        public static bool inited;
        public static event UnityAction OnPlayerInited;

        private static int controlBlockers = 0;
        private static int controlCameraBlockers = 0;
        private static int interactableBlockers = 0;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public LayerMask interactLayerMask = Game.LayerMask.Default;

        [Tooltip("Distance that player can pickup and activate items.")]
        public float interactDistance = 2.1f;

        public float interactCastRadius = 0.05f;

        public Transform audioSources;
        public PlayerMovement controller;
        public CameraBobAnims cameraBobAnims;
        public PlayerFootsteps footsteps;
        public float deadlyHeight = -100f;
        public AudioSource effectsAudioSource;
        public float returnToGroundAltitude = -100f;

        [SerializeField]
        public List<Collider> colliders = new();

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Rigidbody Rigidbody { get; private set; }
        public InputManager InputManager { get; private set; }
        public CapsuleCollider Capsule { get; private set; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // for net sync etc...
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsDead { get; set; }
        public bool IsHidden { get; set; }

        public float Leaning =>
            controller.LeaningComponent.LeanAmt / controller.LeaningComponent.LeaningConfig.leanDistance;

        // PROPERTIES: ----------------------------------------------------------------------------

        public SmoothLook SmoothLook => _smoothLook;
        public CameraControl CameraControl => _cameraControl;
        public GameObject WeaponRoot => _weaponRoot;
        public GameObject CameraRoot => _cameraRoot;
        public Animator CameraAnimator => _cameraAnimator;

        // FIELDS: --------------------------------------------------------------------------------

        private Transform _transform;
        private SmoothLook _smoothLook;
        private CameraControl _cameraControl;
        private Camera _playerCamera;
        private GameObject _weaponRoot;
        private GameObject _cameraRoot;
        private Animator _cameraAnimator;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            FindColliders();
        }

        private void FindColliders()
        {
            colliders.Clear();
            colliders.AddRange(GetComponentsInChildren<Collider>());
        }
#endif

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Capsule = GetComponent<CapsuleCollider>();
            InputManager = InputManager.instance;
            _transform = transform;
        }

        private void Update()
        {
            if (controller.IsFalling && _transform.position.y < deadlyHeight)
            {
                //Kill();
                return;
            }

            if (_transform.position.y < returnToGroundAltitude)
            {
                ReturnToLastGroundPosition();
            }

            if (canInteractable)
            {
                Interactable();
            }
        }

        private void OnDestroy()
        {
            OnPlayerInited = null;
            instance = null;
            inited = false;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(SmoothLook smoothLook, CameraControl cameraControl, Camera playerCamera,
            GameObject weaponRoot, GameObject cameraRoot, Animator cameraAnimator)
        {
            instance = this;
            _smoothLook = smoothLook;
            _cameraControl = cameraControl;
            _playerCamera = playerCamera;
            _weaponRoot = weaponRoot;
            _cameraRoot = cameraRoot;
            _cameraAnimator = cameraAnimator;
            
            SetControl(false, true);
            SetInteractable(false, true);
            OnPlayerInited?.Invoke();
            cameraBobAnims.PlayIdleAnim();
            _cameraControl.SetMainFilter(CameraControl.ScreenEffectProfileType.Gameplay);
            _cameraControl.RemoveEffectFilter();
            _cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Fade, 1f, 1f);
            _cameraControl.enabled = true;
            controller.SpeedMult = 1f;
            audioSources.SetParent(_cameraRoot.transform);
            inited = true;
            
            this.WaitAndCall(0.5f, () =>
            {
                SetControl(true, true);
                SetInteractable(true, true);
                Teleport(_transform.position, _transform.rotation);
            });
        }
        
        public void Teleport(Vector3 position, Quaternion? rotation = null)
        {
            _cameraControl.smooth = false;
            _smoothLook.Smooth = false;
            controller.IsFalling = false;
            _transform.position = position;

            if (rotation != null) Rotate((Quaternion)rotation);

            this.WaitAndCall(1f, () =>
            {
                _cameraControl.smooth = true;
                _smoothLook.Smooth = true;
            });
        }

        public void Rotate(Quaternion rotation)
        {
            Quaternion newRotation = rotation;

            Vector3 playerEulerAngles = new Vector3(0f, newRotation.eulerAngles.y, 0f);
            _transform.eulerAngles = playerEulerAngles;

            Vector3 tempEulerAngles2 = new Vector3(0f, newRotation.eulerAngles.y, 0f);

            _smoothLook.RotationX = 0f;
            _smoothLook.RotationY = 0f;
            _smoothLook.RotationZ = 0f;
            _smoothLook.RecoilY = 0f;
            _smoothLook.RecoilX = 0f;
            _smoothLook.InputY = 0f;

            _smoothLook.OriginalRotation = Quaternion.Euler(tempEulerAngles2);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Interactable()
        {
            GameObject interactable = null;

            RaycastHit hitUsable;

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);

            Physics.Raycast(ray, out hitUsable, interactDistance, interactLayerMask, QueryTriggerInteraction.Ignore);

            if (hitUsable.collider != null && hitUsable.collider.gameObject.CompareTag(Game.Tags.Interactable))
            {
                interactable = hitUsable.collider.gameObject;
            }
            else
            {
                RaycastHit hitUsable2;
                Physics.SphereCast(ray.origin, interactCastRadius, ray.direction, out hitUsable2, interactDistance,
                    interactLayerMask, QueryTriggerInteraction.Ignore);
                
                if (hitUsable2.collider != null)
                {
                    Vector3 center = hitUsable.collider != null
                        ? GameUtils.GetClosestPointOnLine(hitUsable2.point, ray.origin, hitUsable.point)
                        : GameUtils.GetClosestPointOnRay(hitUsable2.point, ray.origin, ray.direction);
                    
                    Collider[] usables = Physics.OverlapSphere(center, interactCastRadius, interactLayerMask,
                        QueryTriggerInteraction.Ignore);
                    
                    interactable = usables.Where(x => x.gameObject.CompareTag(Game.Tags.Interactable))
                        .OrderBy(x => Vector3.Distance(x.ClosestPoint(center), center)).Select(x => x.gameObject)
                        .FirstOrDefault();
                }
            }

            if (interactable != null)
            {
                InteractReceiver interactReceiver = interactable.GetComponent<InteractReceiver>();
                if (interactReceiver != null && interactReceiver.enabled)
                {
                    if (InputManager.GetActionKeyUp(InputManager.Action.Interact))
                    {
                        interactReceiver.InteractRequest();
                    }
                    else
                    {
                        interactReceiver.HoverRequest();
                    }
                }
            }
        }

        private void ReturnToLastGroundPosition()
        {
            Vector3 lastPosOnNavmesh = controller.LastOnGroundPosition;
            if (NavMesh.SamplePosition(lastPosOnNavmesh, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                lastPosOnNavmesh = hit.position;
            }

            lastPosOnNavmesh += Vector3.up * (controller.standingCamHeight + 0.5f);
            _cameraControl.SetEffectFilter(CameraControl.ScreenEffectProfileType.Fade, 1f, 0.75f);
            Teleport(lastPosOnNavmesh);
        }

        private void Kill()
        {
            // disable player control and sprinting on death
            controller.InputX = 0f;
            controller.InputY = 0f;

            controller.Rigidbody.velocity = Vector3.zero;

            cameraBobAnims.PlayIdleAnim();

            SetControl(false, true);
            SetInteractable(false, true);
        }

        private static void SetControl(bool value, bool force = false)
        {
            SetMovementControl(value, force);
            SetCameraControl(value, force);
        }

        private static void SetMovementControl(bool value, bool force = false)
        {
            controlBlockers = Mathf.Max(value ? (force ? 0 : controlBlockers - 1) : (force ? 1 : controlBlockers + 1),
                0);
            canControl = controlBlockers == 0;
            if (!canControl)
            {
                instance.controller.Stop();
                instance.Rigidbody.Sleep();
                instance.cameraBobAnims.PlayIdleAnim();
            }
        }

        private static void SetCameraControl(bool value, bool force = false)
        {
            controlCameraBlockers =
                Mathf.Max(value ? (force ? 0 : controlCameraBlockers - 1) : (force ? 1 : controlCameraBlockers + 1), 0);
            canCameraControl = controlCameraBlockers == 0;
        }

        private static void SetInteractable(bool value, bool force = false)
        {
            interactableBlockers =
                Mathf.Max(value ? (force ? 0 : interactableBlockers - 1) : (force ? 1 : interactableBlockers + 1), 0);
            canInteractable = interactableBlockers == 0;
        }
    }
}