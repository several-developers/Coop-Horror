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
        private Image _greenPingImage;

        [SerializeField, Required]
        private Image _yellowPingImage;

        // FIELDS: --------------------------------------------------------------------------------

        private Tweener _fadeTN;
        private bool _useGreenPing;
        private bool _isAnimationPlaying;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void PlayAnimation()
        {
            if (_isAnimationPlaying)
                return;
            
            StopAnimation();
            Fade(fadeIn: true);
            
            _isAnimationPlaying = true;
        }

        public void StopAnimation()
        {
            _fadeTN.Kill();

            Color greenColor = _greenPingImage.color;
            Color yellowColor = _yellowPingImage.color;
            
            greenColor.a = 0f;
            yellowColor.a = 0f;
            
            _greenPingImage.color = greenColor;
            _yellowPingImage.color = yellowColor;
            
            _isAnimationPlaying = false;
        }

        public void ToggleUseGreenPing(bool useGreenPing) =>
            _useGreenPing = useGreenPing;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void Fade(bool fadeIn)
        {
            Image image = _useGreenPing ? _greenPingImage : _yellowPingImage;
            float endValue = fadeIn ? _maxAlpha : 0f;

            _fadeTN = image
                .DOFade(endValue, _fadeDuration)
                .SetEase(_fadeEase)
                .SetLink(gameObject)
                .OnComplete(() => Fade(!fadeIn));
        }
    }
}