using DG.Tweening;
using GameCore.Infrastructure.Services.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Other
{
    public class ScenesLoaderScreen : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService2 scenesLoaderService2)
        {
            _scenesLoaderService2 = scenesLoaderService2;

            _scenesLoaderService2.OnLoadingScreenStateChangedEvent += OnLoadingScreenStateChanged;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _fadeTime;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Canvas _canvas;

        [SerializeField, Required]
        private CanvasGroup _canvasGroup;

        // FIELDS: --------------------------------------------------------------------------------

        private IScenesLoaderService2 _scenesLoaderService2;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => DontDestroyOnLoad(gameObject);

        private void OnDestroy() =>
            _scenesLoaderService2.OnLoadingScreenStateChangedEvent -= OnLoadingScreenStateChanged;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void FadeAnimation(bool fadeIn)
        {
            if (fadeIn)
                _canvas.enabled = true;

            float value = fadeIn ? 1 : 0;

            _canvasGroup
                .DOFade(value, _fadeTime)
                .OnComplete(() =>
                {
                    if (!fadeIn)
                        _canvas.enabled = false;
                });
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnLoadingScreenStateChanged(bool showLoadingScreen) => FadeAnimation(showLoadingScreen);
    }
}