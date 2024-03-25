using DG.Tweening;
using GameCore.Infrastructure.Services.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Other
{
    public class LoadingScreenView : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService scenesLoaderService)
        {
            _scenesLoaderService = scenesLoaderService;

            _scenesLoaderService.OnLoadingScreenStateChangedEvent += OnLoadingScreenStateChanged;
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

        private IScenesLoaderService _scenesLoaderService;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => DontDestroyOnLoad(gameObject);

        private void OnDestroy() =>
            _scenesLoaderService.OnLoadingScreenStateChangedEvent -= OnLoadingScreenStateChanged;

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