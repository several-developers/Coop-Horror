using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EFPController.Utils;

namespace EFPController
{
    public static class CameraAnimNames
    {
        public const string Idle = "Idle";
        public const string Jump = "Jump";
        public const string Land = "Land";
        public const string Climb = "Climb";
    }

    public class CameraControl : MonoBehaviour
    {
        public enum ScreenEffectProfileType
        {
            None,
            Fade,
            Gameplay,
            Teleport,
            Dash,
        }

        [System.Serializable]
        public class ScreenEffectProfile
        {
            public ScreenEffectProfileType type;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Camera mainCamera;
        public List<ScreenEffectProfile> effectProfiles = new List<ScreenEffectProfile>();

        [Tooltip("Speed to smooth the camera angles.")]
        public float camSmoothSpeed = 0.075f;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HideInInspector]
        public Vector3
            CameraAnglesAnim =
                Vector3.zero; // DONT RENAME! USES IN ANIMATIONS. these values are modified by animations and added to camera angles

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool smooth { get; set; } = true;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // to move gun and view down slightly on contact with ground
        private bool landState = false;
        private float landStartTime = 0f;
        private float landTime = 0.35f;

        // camera roll angle smoothing amounts
        private float rollAmt;
        private float lookRollAmt;

        private Player _player;
        private Transform _playerTransform;
        private Animator _animator;
        private Quaternion tempCamAngles;
        private float deltaAmt;
        private float returnSpeedAmt = 4f; // speed that camera angles return to neutral
        private Vector3 dampVel;
        private float lerpSpeedAmt;
        private float movingTime; // for controlling delay in lerp of camera position
        private Vector3 targetPos = Vector3.one; // point to orbit camera around in third person mode
        private Vector3 camPos; // camera position
        private Vector3 tempLerpPos = Vector3.one;
        private float dampVelocity;
        private float dampOrg; // smoothed vertical camera postion
        
        private PlayerMovement _controller;
        private CameraBobAnims _cameraBobAnims;
        private CameraRootAnimations _cameraRootAnimations;

        void Awake()
        {
            mainCamera = GetComponent<Camera>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            smooth = true;
        }

        public void Update()
        {
            if (!Player.inited || !Player.canCameraControl) return;

            // make sure that animated camera angles zero-out when not playing an animation
            // this is necessary because sometimes the angle amounts did not return to zero
            // which resulted in the gun and camera angles becoming misaligned
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
            {
                CameraAnglesAnim = Vector3.zero;
            }

            // set up camera position with horizontal lean amount
            targetPos = _playerTransform.position + _playerTransform.right * _controller.LeaningComponent.LeanPos;

            // if world has just been recentered, don't lerp camera position to prevent lagging behind player object position
            if (movingTime + 0.75f > Time.time)
            {
                lerpSpeedAmt = 0f;
            }
            else
            {
                lerpSpeedAmt =
                    Mathf.MoveTowards(lerpSpeedAmt, camSmoothSpeed,
                        Time.deltaTime); // gradually change lerpSpeedAmt for smoother lerp transition
            }

            // smooth player position before applying bob effects
            if (smooth)
            {
                tempLerpPos = Vector3.SmoothDamp(tempLerpPos, targetPos, ref dampVel, lerpSpeedAmt, Mathf.Infinity,
                    Time.smoothDeltaTime);
            }
            else
            {
                tempLerpPos = targetPos;
            }

            // side to side bobbing/moving of camera (stored in the dampOriginX) needs to added to the right vector
            // of the transform so that the bobbing amount is correctly applied along the X and Z axis.
            // If added just to the x axis, like done with the vertical Y axis, the bobbing does not rotate
            // with camera/mouselook and only moves on the world axis.
            if (smooth)
            {
                dampOrg = Mathf.SmoothDamp(dampOrg, _controller.MidPos, ref dampVelocity, _controller.CamDampSpeed,
                    Mathf.Infinity, Time.smoothDeltaTime);
            }
            else
            {
                dampOrg = _controller.MidPos;
            }

            camPos = tempLerpPos + (_playerTransform.right *
                                    (_cameraRootAnimations.camPosAnim.x * _cameraBobAnims.camPositionBobAmt.x))
                                 + new Vector3(0f,
                                     dampOrg + (_cameraRootAnimations.camPosAnim.y * _cameraBobAnims.camPositionBobAmt.y),
                                     0f /*cameraRootAnimations.camPosAnim.z*/);

            transform.parent.transform.position = camPos;
            transform.position = camPos;

            // initialize camera position/angles quickly before fade out on level load
            if (Time.timeSinceLevelLoad < 0.5f || !smooth)
            {
                returnSpeedAmt = 64f;
            }
            else
            {
                if (_controller.SwimmingComponent.IsBelowWater)
                {
                    returnSpeedAmt = _cameraBobAnims.rollReturnSpeedSwim;
                }
                else
                {
                    returnSpeedAmt = _cameraBobAnims.rollReturnSpeed;
                }
            }

            // caculate camera roll angle amounts
            if (_controller.SprintComponent.Sprint || _controller.DashComponent.DashActive)
            {
                rollAmt = _cameraBobAnims.sprintStrafeRoll;
                // view rolls more with horizontal looking during bullet time for dramatic effect
                lookRollAmt = -1000f * (1f - Time.timeScale);
            }
            else
            {
                rollAmt = _cameraBobAnims.walkStrafeRoll;
                
                if (_controller.SwimmingComponent.IsBelowWater)
                {
                    lookRollAmt = -100f * _cameraBobAnims.swimLookRoll;
                }
                else
                {
                    if (Time.timeScale < 1f)
                    {
                        // view rolls more with horizontal looking during bullet time for dramatic effect
                        lookRollAmt = -500f * (1f - Time.timeScale);
                    }
                    else
                    {
                        lookRollAmt = -100f * _cameraBobAnims.lookRoll;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Camera Angle Assignment
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			

            // apply a force to the camera that returns it to neutral angles (Quaternion.identity) over time after being changed by code or by animations
            if (smooth)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity,
                    Time.deltaTime * returnSpeedAmt);
            }
            else
            {
                transform.localRotation = Quaternion.identity;
            }

            deltaAmt = Mathf.Clamp01(Time.deltaTime) * 75f;

            Vector3 CameraAnglesAnimTemp = CameraAnglesAnim;
            // store camera angles in temporary Quaternion and add yaw and pitch from animations 
            tempCamAngles = Quaternion.Euler(transform.localEulerAngles.x -
                                             (_cameraRootAnimations.camAngleAnim.y * _cameraBobAnims.camAngleBobAmt.y *
                                              deltaAmt)
                                             + (CameraAnglesAnimTemp.x * deltaAmt), // camera pitch modifiers
                transform.localEulerAngles.y -
                (_cameraRootAnimations.camAngleAnim.x * _cameraBobAnims.camAngleBobAmt.x * deltaAmt)
                + (CameraAnglesAnimTemp.y * deltaAmt), // camera yaw modifiers
                transform.localEulerAngles.z -
                (_cameraRootAnimations.camAngleAnim.z * _cameraBobAnims.camAngleBobAmt.z * deltaAmt)
                + (CameraAnglesAnimTemp.z * deltaAmt) // camera roll modifiers 
                - (_controller.LeaningComponent.LeanAmt * 3f * Time.deltaTime * returnSpeedAmt)
                - (_controller.InputX * rollAmt * Time.deltaTime * returnSpeedAmt)
                - (_cameraBobAnims.side * lookRollAmt * Time.deltaTime * returnSpeedAmt));

            // apply tempCamAngles to camera angles
            transform.localRotation = tempCamAngles;

            // Track time that player has landed from jump or fall for gun kicks
            if (_controller.FallingDistance < 1.25f && !_controller.JumpingComponent.IsJumping)
            {
                if (!landState)
                {
                    // init timer amount
                    landStartTime = Time.time;
                    // set landState only once
                    landState = true;
                }
            }
            else
            {
                if (landState)
                {
                    // if land time has elapsed
                    if (landStartTime + landTime < Time.time)
                    {
                        // reset landState
                        landState = false;
                    }
                }
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Init(Player player, CameraRootAnimations cameraRootAnimations)
        {
            _player = player;
            _playerTransform = player.transform;
            _controller = player.controller;
            _cameraBobAnims = player.cameraBobAnims;
            _cameraRootAnimations = cameraRootAnimations;
        }
        
        public void SetMainFilter(ScreenEffectProfileType effectType)
        {
            if (effectType == ScreenEffectProfileType.None) return;
            ScreenEffectProfile effectProfile = effectProfiles.FirstOrDefault(x => x.type == effectType);
        }

        private Coroutine effectCoroutine;

        public void SetEffectFilter(ScreenEffectProfileType effectType, float weight, float fadeOutTime,
            float fadeInTime = 0f, float effectTime = 0f)
        {
            if (effectType == ScreenEffectProfileType.None)
            {
                return;
            }

            ScreenEffectProfile effectProfile = effectProfiles.FirstOrDefault(x => x.type == effectType);

            if (effectProfile == null)
            {
                Debug.LogError("Can't find profile \"" + effectType.ToString() + "\" in effect profiles list");
                return;
            }


            if (effectCoroutine != null)
                StopCoroutine(effectCoroutine);
            if (fadeInTime > 0f)
            {
                effectCoroutine = StartCoroutine(EffectFadeInCoroutine(fadeInTime,
                    () =>
                    {
                        this.WaitAndCall(effectTime,
                            () => { effectCoroutine = StartCoroutine(EffectFadeOutCoroutine(fadeOutTime)); });
                    }));
            }
            else
            {
                this.WaitAndCall(effectTime,
                    () => { effectCoroutine = StartCoroutine(EffectFadeOutCoroutine(fadeOutTime)); });
            }
        }

        public void RemoveEffectFilter()
        {
            if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        }

        private IEnumerator EffectFadeInCoroutine(float time, System.Action callback = null)
        {
            yield return new WaitForEndOfFrame();

            callback?.Invoke();
        }

        private IEnumerator EffectFadeOutCoroutine(float time)
        {
            yield return new WaitForEndOfFrame();
            RemoveEffectFilter();
        }
    }
}