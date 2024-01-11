using UnityEngine;
using System.Collections;
using EFPController.Utils;

namespace EFPController
{
    public class PlayerFootsteps : MonoBehaviour
    {
        [System.Serializable]
        public struct SurfaceDefinition
        {
            public int surfaceIndex;
            public AudioClip[] footsteps;
            public AudioClip[] landing;
        }

        public AudioClip[] waterFootsteps;

        public SurfaceDefinition[] definedSurfaces;

        public bool allowAnimEvent = false;
        public int animationsInBlend = 1;
        public float distanceBetweenStepsWalk = 1.8f;
        public float distanceBetweenStepsSlow = 1.8f;
        public float distanceBetweenStepsSprint = 1.8f;
        public float distanceBetweenStepsDash = 1.8f;
        public int defaultSurfaceIndex = 0;

        [Tooltip("You need an audio source to play a footstep sound.")]
        public AudioSource audioSource;

        [Tooltip("Random volume between this limits")]
        public Vector2 volume = new Vector2(0.25f, 0.5f);

        public float landVolumeMult = 2f;
        public float silentVolumeMult = 0.5f;
        public float sprintVolumeMult = 1.2f;
        public float dashVolumeMult = 1.8f;

        private RaycastHit currentGroundInfo;
        private bool isGrounded;
        private bool previouslyGrounded;
        private bool firstGrounded = false;

        private float stepCycleProgress;
        private float distanceBetweenSteps;

        public bool isCrouch { get; set; } = false;
        public bool isSprint { get; set; } = false;
        public bool isDash { get; set; } = false;

        private bool inited = false;

        private float currentVolume
        {
            get
            {
                float temp = volume.Random();
                if (isSprint) temp *= sprintVolumeMult;
                if (isDash) temp *= dashVolumeMult;
                return temp;
            }
        }

        private Player player;

        private void Awake()
        {
            player = GetComponent<Player>();
        }

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => Player.instance == null);
            yield return new WaitForSeconds(1f);
            player.controller.OnWater += Controller_OnWater;
            inited = true;
        }

        private void Controller_OnWater(bool inWater)
        {
            if (!inWater)
                return;

            PlayerMovement playerMovement = player.controller;

            if (playerMovement.IsGrounded && !playerMovement.IsFalling && !playerMovement.JumpingComponent.IsJumping)
            {
                stepCycleProgress = 0f;
                PlayFootstep();
            }
        }

        public void Footstep(AnimationEvent evt)
        {
            if (!allowAnimEvent) return;
            if (evt.animatorClipInfo.weight < 1f / animationsInBlend) return;
            if (isGrounded) PlayFootstep();
        }

        private void AdvanceStepCycle(float increment)
        {
            stepCycleProgress += increment;
            float dist = distanceBetweenStepsWalk;
            if (isCrouch) dist = distanceBetweenStepsSlow;
            if (isSprint) dist = distanceBetweenStepsSprint;
            if (isDash) dist = distanceBetweenStepsDash;

            distanceBetweenSteps = Mathf.Lerp(distanceBetweenSteps, dist, 5f * Time.deltaTime);

            if (stepCycleProgress > distanceBetweenSteps)
            {
                stepCycleProgress = 0f;
                PlayFootstep();
            }
        }

        private void PlayFootstep()
        {
            AudioClip randomFootstep = GetFootstep(currentGroundInfo.collider, currentGroundInfo.point);
            if (randomFootstep) audioSource.PlayOneShot(randomFootstep, currentVolume);
        }

        private void PlayLandSound()
        {
            AudioClip randomFootstep = GetFootstep(currentGroundInfo.collider, currentGroundInfo.point);
            if (randomFootstep) audioSource.PlayOneShot(randomFootstep, volume.Random() * landVolumeMult);
        }

        private void CheckGround()
        {
            previouslyGrounded = isGrounded;
            currentGroundInfo = player.controller.GroundHitInfo;
            isGrounded = player.controller.IsGrounded
                         && !player.controller.JumpingComponent.IsJumping
                         && !player.controller.IsFalling
                         && !player.controller.SwimmingComponent.IsSwimming;

            if (previouslyGrounded || !isGrounded)
                return;
            
            if (firstGrounded)
                PlayLandSound();
            else
                firstGrounded = true;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void UpdateLogic()
        {
            if (!inited || player.controller.SwimmingComponent.IsSwimming)
                return;

            isCrouch = player.controller.CrouchingComponent.IsCrouching;
            isSprint = player.controller.SprintComponent.Sprint;
            isDash = player.controller.DashComponent.DashActive;
            CheckGround();

            if (isGrounded && previouslyGrounded && (player.controller.IsMoving || isDash))
                AdvanceStepCycle(player.controller.Velocity.magnitude * Time.deltaTime);
        }
        
        public AudioClip GetFootstep(Collider groundCollider, Vector3 worldPosition)
        {
            if (player.controller.SwimmingComponent.InWater)
                return waterFootsteps.Random();

            int surfaceIndex = SurfaceManager.instance.GetSurfaceIndex(groundCollider, worldPosition);

            if (surfaceIndex == -1 || definedSurfaces.Length <= surfaceIndex)
                surfaceIndex = defaultSurfaceIndex;

            return definedSurfaces[surfaceIndex].footsteps.Random();
        }
    }
}