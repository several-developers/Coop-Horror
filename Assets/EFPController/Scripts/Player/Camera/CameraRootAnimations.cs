using EFPController.Utils;
using UnityEngine;

namespace EFPController
{
    public static class CameraRootAnimNames
    {
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Sprint = "Sprint";
        public const string Climbing = "Climbing";
    }

    public class CameraRootAnimations : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        // DONT RENAME! USES IN ANIMATIONS.
        public Vector3 camPosAnim;
        public Vector3 camAngleAnim;
        public Vector3 weapPosAnim;
        public Vector3 weapAngleAnim;

        public float revertSpeed = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------
        
        public Animator Animator { get; private set; }

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake() =>
            Animator = GetComponent<Animator>();

        private void Update()
        {
            if (Animator.speed.IsZero())
            {
                Animator.enabled = false;
                camPosAnim = Vector3.Lerp(camPosAnim, Vector3.zero, revertSpeed * Time.deltaTime);
                camAngleAnim = Vector3.Lerp(camAngleAnim, Vector3.zero, revertSpeed * Time.deltaTime);
                weapPosAnim = Vector3.Lerp(weapPosAnim, Vector3.zero, revertSpeed * Time.deltaTime);
                weapAngleAnim = Vector3.Lerp(weapAngleAnim, Vector3.zero, revertSpeed * Time.deltaTime);
            }
            else
            {
                Animator.enabled = true;
            }
        }
    }
}