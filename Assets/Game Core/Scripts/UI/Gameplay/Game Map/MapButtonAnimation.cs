using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI.Gameplay.GameMap
{
    public class MapButtonAnimation : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _fadeDuration = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float _maxAlpha = 0.3f;

        [SerializeField]
        private Ease _fadeEase = Ease.InOutQuad;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Image _pingImage;

        // FIELDS: --------------------------------------------------------------------------------

        private Tweener _fadeTN;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlayAnimation()
        {
            StopAnimation();
            Fade(fadeIn: true);
        }

        public void StopAnimation()
        {
            _fadeTN.Kill();

            Color color = _pingImage.color;
            color.a = 0f;
            _pingImage.color = color;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Fade(bool fadeIn)
        {
            float endValue = fadeIn ? _maxAlpha : 0f;

            _fadeTN = _pingImage
                .DOFade(endValue, _fadeDuration)
                .SetEase(_fadeEase)
                .SetLink(gameObject)
                .OnComplete(() => Fade(!fadeIn));
        }
    }
}