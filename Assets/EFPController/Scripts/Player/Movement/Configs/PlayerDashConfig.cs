using GameCore;
using MetaEditor;
using UnityEngine;

namespace EFPController
{
    [CreateAssetMenu]
    public class PlayerDashConfig : EditorMeta
    {
        [Header("Dash")]
        public bool allowDash = false;

        public float dashSpeed = 15f;

        public AnimationCurve dashSpeedCurve = new()
            { keys = new Keyframe[] { new(0f, 1f), new(1f, 0f) } };

        public float dashTime = 1f;
        public float dashActiveTime = 0.75f;
        public float dashCooldown = 1f;
        public float dashSoundVolume = 0.5f;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}