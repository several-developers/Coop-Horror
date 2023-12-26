using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.Global.Animations
{
    [Serializable]
    public class ScaleAnimation
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Min(0)]
        private float _targetScale = 1.1f;

        [SerializeField, Min(0)]
        private float _scaleUpDuration = 0.2f;
        
        [SerializeField, Min(0)]
        private float _scaleDownDuration = 0.2f;

        [SerializeField]
        private Ease _ease = Ease.InOutQuad;

        [SerializeField, Required]
        private RectTransform _scaleRT;

        // FIELDS: --------------------------------------------------------------------------------

        private GameObject _owner;
        private Tweener _scaleTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetOwner(GameObject owner) =>
            _owner = owner;

        public void ScaleUp()
        {
            StopAnimation();
            Scale(_targetScale, _scaleUpDuration);
        }

        public void ScaleDown()
        {
            StopAnimation();
            Scale(endValue: 1f, _scaleDownDuration);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopAnimation() =>
            _scaleTN.Kill();

        private void Scale(float endValue, float duration)
        {
            _scaleTN = _scaleRT
                .DOScale(endValue, duration)
                .SetEase(_ease)
                .SetLink(_owner);
        }
    }
}