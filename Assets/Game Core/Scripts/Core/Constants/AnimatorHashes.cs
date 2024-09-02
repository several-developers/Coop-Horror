using UnityEngine;

namespace GameCore
{
    public static class AnimatorHashes
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public static readonly int Speed = Animator.StringToHash(name: "Speed");
        public static readonly int MoveSpeed = Animator.StringToHash(name: "MoveSpeed");
        public static readonly int MotionSpeed = Animator.StringToHash(name: "MotionSpeed");
        public static readonly int CanMove = Animator.StringToHash(name: "CanMove");
        public static readonly int HitReaction = Animator.StringToHash(name: "HitReaction");
        public static readonly int Attack = Animator.StringToHash(name: "Attack");
        public static readonly int Reload = Animator.StringToHash(name: "Reload");
        public static readonly int ReloadMultiplier = Animator.StringToHash(name: "ReloadMultiplier");
        public static readonly int Scream = Animator.StringToHash(name: "Scream");
        public static readonly int Tweet = Animator.StringToHash(name: "Tweet");
        public static readonly int Open = Animator.StringToHash(name: "Open");
        public static readonly int Close = Animator.StringToHash(name: "Close");
        public static readonly int Trigger = Animator.StringToHash(name: "Trigger");
        public static readonly int Aggressiveness = Animator.StringToHash(name: "Aggressiveness");
        public static readonly int Suspicion = Animator.StringToHash(name: "Suspicion");
        public static readonly int IdleType = Animator.StringToHash(name: "IdleType");
        public static readonly int RunningType = Animator.StringToHash(name: "RunningType");
        public static readonly int MoveSpeedMultiplier = Animator.StringToHash(name: "MoveSpeedMultiplier");
        
        public static readonly int IsWalking = Animator.StringToHash(name: "IsWalking");
        public static readonly int IsSprinting = Animator.StringToHash(name: "IsSprinting");
        public static readonly int IsScreaming = Animator.StringToHash(name: "IsScreaming");
        public static readonly int IsCrawling = Animator.StringToHash(name: "IsCrawling");
        public static readonly int IsAgonizing = Animator.StringToHash(name: "IsAgonizing");
        public static readonly int IsSitting = Animator.StringToHash(name: "IsSitting");
        public static readonly int IsOn = Animator.StringToHash(name: "IsOn");
        public static readonly int IsOpen = Animator.StringToHash(name: "IsOpen");
        
        public const string ReloadingAnimation = "Reloading";
    }
}