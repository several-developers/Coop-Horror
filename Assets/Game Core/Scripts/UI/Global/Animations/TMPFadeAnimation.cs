using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Global.Animations
{
    [Serializable]
    public class TMPFadeAnimation
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Min(0)]
        private float _fadeInDuration = 0.2f;
        
        [SerializeField, Min(0)]
        private float _fadeOutDuration = 0.2f;

        [SerializeField]
        private Ease _ease = Ease.InOutQuad;

        [SerializeField, Required]
        private TextMeshProUGUI _targetTMP;

        // FIELDS: --------------------------------------------------------------------------------

        private GameObject _owner;
        private Tweener _scaleTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetOwner(GameObject owner) =>
            _owner = owner;

        public void Show()
        {
            StopAnimation();
            Fade(endValue: 1f, _fadeInDuration);
        }

        public void Hide()
        {
            StopAnimation();
            Fade(endValue: 0f, _fadeOutDuration);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopAnimation() =>
            _scaleTN.Kill();

        private void Fade(float endValue, float duration)
        {
            _scaleTN = _targetTMP
                .DOFade(endValue, duration)
                .SetEase(_ease)
                .SetLink(_owner);
        }
    }
}