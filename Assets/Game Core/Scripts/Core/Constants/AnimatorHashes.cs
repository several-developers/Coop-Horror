using UnityEngine;

namespace GameCore
{
    public static class AnimatorHashes
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");
        public static readonly int MoveSpeedBlend = Animator.StringToHash("MoveSpeedBlend");
        public static readonly int CanMove = Animator.StringToHash("CanMove");
        public static readonly int HitReaction = Animator.StringToHash("HitReaction");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Reload = Animator.StringToHash("Reload");
        public static readonly int ReloadMultiplier = Animator.StringToHash("ReloadMultiplier");
        public static readonly int Scream = Animator.StringToHash("Scream");
        
        public static readonly int IsSprinting = Animator.StringToHash("IsSprinting");
        public static readonly int IsCrawling = Animator.StringToHash("IsCrawling");
        public static readonly int IsOn = Animator.StringToHash("IsOn");
        public static readonly int IsOpen = Animator.StringToHash("IsOpen");
        
        public const string ReloadingAnimation = "Reloading";
    }
}