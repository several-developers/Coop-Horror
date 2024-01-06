using UnityEngine;
using System.Collections;
using EFPController.Utils;

namespace EFPController
{
    [DefaultExecutionOrder(-9)]
    public class CameraBobAnims : MonoBehaviour
    {
        public SmoothLook smoothLook { get; private set; }
        public CameraRootAnimations cameraRootAnimations { get; private set; }
        private PlayerMovement controller;
        private Player player;
        private InputManager inputManager;

        private CameraControl CameraControlComponent;

        // camera FOV handling
        [Tooltip("Default camera field of view value.")]
        public float defaultFov = 75f;

        [Tooltip("Default camera field of view value while sprinting.")]
        public float sprintFov = 85f;

        [Tooltip("Amount to subtract from main camera FOV for weapon camera FOV.")]
        public float weaponCamFovDiff = 20f; // amount to subtract from main camera FOV for weapon camera FOV

        [HideInInspector]
        public float nextFov = 75f; // camera field of view that is smoothed using smoothDamp

        [HideInInspector]
        public float newFov = 75f; // camera field of view that is smoothed using smoothDamp

        private float FovSmoothSpeed = 0.15f; // speed that camera FOV is smoothed
        private float dampFOV = 0f; // target weapon z position that is smoothed using smoothDamp function

        [Header("Bobbing Speeds and Amounts")]
        // speeds and camera bobbing amounts for player movement states	
        [Tooltip("Camera position bobbing amount when walking (X = horizontal, Y = vertical).")]
        public Vector2 walkPositionBob = Vector2.one;

        [Tooltip("Camera angle bobbing amount when walking (yaw, pitch, roll).")]
        public Vector3 walkAngleBob = Vector3.one;

        [Tooltip("Camera and weapon bobbing speed when walking.")]
        public float walkBobSpeed = 1f;

        [Tooltip("Camera position bobbing amount when sprinting (X = horizontal, Y = vertical).")]
        public Vector2 sprintPositionBob = Vector2.one;

        [Tooltip("Camera angle bobbing amount when sprinting (yaw, pitch, roll).")]
        public Vector3 sprintAngleBob = Vector3.one;

        [Tooltip("Camera and weapon bobbing speed when sprinting.")]
        public float sprintBobSpeed = 1f;

        [Tooltip("Camera position bobbing amount when crouching (X = horizontal, Y = vertical).")]
        public Vector2 crouchPositionBob = Vector2.one;

        [Tooltip("Camera angle bobbing amount when crouching (yaw, pitch, roll).")]
        public Vector3 crouchAngleBob = Vector3.one;

        [Tooltip("Camera and weapon bobbing speed when crouching.")]
        public float crouchBobSpeed = 1f;

        [Tooltip("Camera and weapon bobbing speed multiplier when swimming.")]
        public float swimBobSpeedFactor = 0.6f;

        private float swimBobSpeedAmt = 0f;
        private float moveInputAmt = 0f;
        private float moveInputSpeed = 0f;

        // actual bobbing vars that are passed to position and rotation calculations
        // these are switched to above amounts depending on movement state
        [HideInInspector]
        public Vector2 camPositionBobAmt = Vector2.zero;

        [HideInInspector]
        public Vector3 camAngleBobAmt = Vector3.zero;

        [Tooltip("Amount to roll the screen left or right when strafing and sprinting.")]
        public float sprintStrafeRoll = 2f;

        [Tooltip("Amount to roll the screen left or right when strafing and walking.")]
        public float walkStrafeRoll = 1f;

        [Tooltip("Amount to roll the screen left or right when moving view horizontally.")]
        public float lookRoll = 1f;

        [Tooltip("Amount to roll the screen left or right when moving view horizontally and underwater.")]
        public float swimLookRoll = 1f;

        [Tooltip("Speed to return to neutral roll values when above water.")]
        public float rollReturnSpeed = 4f;

        [Tooltip("Speed to return to neutral roll values when underwater.")]
        public float rollReturnSpeedSwim = 2f;

        [HideInInspector]
        public float switchMove = 0f; // for moving weapon down while switching weapons

        [HideInInspector]
        public float climbMove = 0f; // for moving weapon down while climbing

        [HideInInspector]
        public float jumpAmt = 0f;

        [HideInInspector]
        public float side = 0f; // amount to sway weapon position horizontally

        [HideInInspector]
        public float raise = 0f; // amount to sway weapon position vertically

        private void Awake()
        {
            inputManager = InputManager.instance;
            player = GetComponent<Player>();
            smoothLook = player.smoothLook;
            controller = player.controller;
            cameraRootAnimations = player.cameraRootAnimations;
            CameraControlComponent = player.cameraControl;
        }

        private void Start() => PlayIdleAnim();

        private void Update()
        {
            // smooth camera FOV
            newFov = Mathf.SmoothDamp(CameraControlComponent.mainCamera.fieldOfView, nextFov, ref dampFOV,
                FovSmoothSpeed, Mathf.Infinity, Time.deltaTime);

            CameraControlComponent.mainCamera.fieldOfView = newFov;

            // Get input from player movement script
            float horizontal = controller.InputX;
            float vertical = controller.InputY;
            bool forwardSprintOnly = controller.SprintComponent.SprintConfig.forwardSprintOnly;

            if (controller.IsMoving)
            {
                bool isSprintingOrDashing = controller.SprintComponent.Sprint || controller.DashComponent.DashActive;
                bool isCrouching = controller.CrouchingComponent.IsCrouching;
                
                bool checkOne = isSprintingOrDashing
                                && !isCrouching
                                && controller.MidPos >= controller.standingCamHeight;

                bool checkTwo = !isCrouching && isSprintingOrDashing;
                
                bool checkThree = controller.FallingDistance < 0.75f
                                && !controller.JumpingComponent.IsJumping;
                
                // Vlad, redone this checking from above
                bool isSprinting = (checkOne || checkTwo) && checkThree;

                // Check for sprinting.
                if (isSprinting)
                {
                    // Actually sprinting now.
                    if (controller.FallingDistance < 0.75f && !controller.JumpingComponent.IsJumping)
                    {
                        if (controller.IsGrounded)
                        {
                            PlaySprintAnim();
                        }
                        else
                        {
                            PlayIdleAnim();
                        }
                        
                        // set the camera's fov back to normal if the player has sprinted into a wall, but the sprint is still active
                        if (((controller.InputY != 0f && forwardSprintOnly) ||
                             (!forwardSprintOnly && controller.IsMoving)))
                        {
                            nextFov = sprintFov;
                        }
                        else
                        {
                            nextFov = defaultFov;
                        }
                    }
                    else
                    {
                        // not sprinting
                        nextFov = defaultFov;
                        // make this check to prevent weapon occasionally not lowering during switch while moving 
                        switchMove = 0f;
                    }
                }
                else
                {
                    // walking
                    nextFov = defaultFov;
                    if (controller.ClimbingComponent.IsClimbing)
                    {
                        PlayClimbingAnim();
                    }
                    else if (controller.IsGrounded)
                    {
                        PlayWalkAnim();
                    }
                    else
                    {
                        PlayIdleAnim();
                    }
                }
            }
            else
            {
                // if not moving (no player movement input)
                PlayIdleAnim();
                nextFov = defaultFov;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Adjust vars for zoom and other states
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // use this variable to slow down bobbing speeds or increase forward bobbing amount if player is swimming
            if (controller.SwimmingComponent.IsSwimming)
            {
                swimBobSpeedAmt =
                    Mathf.Max(0.01f, swimBobSpeedFactor); // actual swim bobbing factor, don't allow divide by zero
            }
            else
            {
                swimBobSpeedAmt = 1f;
            }

            // scale the weapon and camera animation speeds with the amount the joystick is pressed
            if (controller.SprintComponent.SprintActive || controller.DashComponent.DashActive)
            {
                moveInputSpeed = 1f;
            }
            else
            {
                moveInputSpeed = Mathf.Clamp01(Mathf.Max(Mathf.Abs(controller.InputX), Mathf.Abs(controller.InputY)));
            }

            // gradually set the animation speed multiplier (moveInputAmt) to prevent jerky transitions between moving and stopping
            moveInputAmt = Mathf.MoveTowards(moveInputAmt, moveInputSpeed, Time.deltaTime * 3.5f);

            // if zoomed check time of weapon sprinting anim to make weapon return to center, then zoom normally

            float sensitivity = smoothLook.currentSensitivity;

            FovSmoothSpeed = 0.18f; // slower FOV zoom speed when zooming out

            // Return mouse sensitivity to normal
            smoothLook.sensitivityAmt = sensitivity;
            
            // Set weapon and view bobbing amounts
            if ((controller.SprintComponent.Sprint || controller.DashComponent.DashActive)
                && !(forwardSprintOnly && (Mathf.Abs(horizontal) != 0f) && (Mathf.Abs(vertical) < 0.75f))
                && (Mathf.Abs(vertical) != 0f || (!forwardSprintOnly && controller.IsMoving))
                && !controller.CrouchingComponent.IsCrouching
                && controller.MidPos >= controller.standingCamHeight)
            {
                // scale up bob speeds slowly to prevent jerky transition
                camPositionBobAmt =
                    Vector2.MoveTowards(camPositionBobAmt, sprintPositionBob, Time.smoothDeltaTime * 16f);
                camAngleBobAmt = Vector3.MoveTowards(camAngleBobAmt, sprintAngleBob, Time.smoothDeltaTime * 16f);

                cameraRootAnimations.Animator.speed = Mathf.MoveTowards(
                    cameraRootAnimations.Animator.speed * moveInputAmt, sprintBobSpeed, Time.smoothDeltaTime * 16f);
            }
            else
            {
                // scale up bob speeds slowly to prevent jerky transition

                if (controller.CrouchingComponent.IsCrouching)
                {
                    // crouching bob amounts
                    camPositionBobAmt =
                        Vector2.MoveTowards(camPositionBobAmt, crouchPositionBob, Time.smoothDeltaTime * 16f);
                    camAngleBobAmt = Vector3.MoveTowards(camAngleBobAmt, crouchAngleBob, Time.smoothDeltaTime * 16f);

                    cameraRootAnimations.Animator.speed = Mathf.MoveTowards(
                        cameraRootAnimations.Animator.speed * moveInputAmt,
                        crouchBobSpeed, Time.smoothDeltaTime * 16f);
                }
                else
                {
                    // walking bob amounts
                    camPositionBobAmt =
                        Vector2.MoveTowards(camPositionBobAmt, walkPositionBob, Time.smoothDeltaTime * 16f);
                    camAngleBobAmt.x = Mathf.MoveTowards(camAngleBobAmt.x, walkAngleBob.x, Time.smoothDeltaTime * 16f);
                    camAngleBobAmt.y = Mathf.MoveTowards(camAngleBobAmt.y, walkAngleBob.y, Time.smoothDeltaTime * 16f);

                    // walk forward bobbing amount, greater if swimming
                    camAngleBobAmt.z = Mathf.MoveTowards(camAngleBobAmt.z, (walkAngleBob.z / swimBobSpeedAmt),
                        Time.smoothDeltaTime * 16f);

                    // walk bobbing speed, slower if swimming
                    cameraRootAnimations.Animator.speed = Mathf.MoveTowards(cameraRootAnimations.Animator.speed,
                        walkBobSpeed * swimBobSpeedAmt * moveInputAmt, Time.smoothDeltaTime * 16f);
                }
            }
        }

        public void PlayIdleAnim()
        {
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Walk))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Walk);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Sprint))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Sprint);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Climbing))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Climbing);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Idle))
                cameraRootAnimations.Animator.SetTrigger(CameraRootAnimNames.Idle);
        }

        public void PlayWalkAnim()
        {
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Idle))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Idle);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Sprint))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Sprint);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Climbing))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Climbing);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Walk))
                cameraRootAnimations.Animator.SetTrigger(CameraRootAnimNames.Walk);
        }

        public void PlaySprintAnim()
        {
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Walk))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Walk);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Idle))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Idle);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Climbing))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Climbing);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Sprint))
                cameraRootAnimations.Animator.SetTrigger(CameraRootAnimNames.Sprint);
        }

        public void PlayClimbingAnim()
        {
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Walk))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Walk);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Idle))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Idle);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Sprint))
                cameraRootAnimations.Animator.ResetTrigger(CameraRootAnimNames.Sprint);
            if (cameraRootAnimations.Animator.HasParameter(CameraRootAnimNames.Climbing))
                cameraRootAnimations.Animator.SetTrigger(CameraRootAnimNames.Climbing);
        }
    }
}