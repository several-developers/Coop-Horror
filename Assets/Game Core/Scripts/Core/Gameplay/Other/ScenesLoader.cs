using DG.Tweening;
using GameCore.Enums;
using GameCore.Infrastructure.Services.Global;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Other
{
    public class ScenesLoader : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService scenesLoaderService)
        {
            _scenesLoaderService = scenesLoaderService;

            _scenesLoaderService.OnSceneStartLoading += OnSceneStartLoading;
            _scenesLoaderService.OnSceneFinishedLoading += OnSceneFinishedLoading;
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

        private void OnDestroy()
        {
            _scenesLoaderService.OnSceneStartLoading -= OnSceneStartLoading;
            _scenesLoaderService.OnSceneFinishedLoading -= OnSceneFinishedLoading;
        }

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

        private void OnSceneStartLoading() =>
            FadeAnimation(fadeIn: true);

        private void OnSceneFinishedLoading() =>
            FadeAnimation(fadeIn: false);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(25), DisableInEditorMode]
        private void DebugLoadMainMenu() =>
            _scenesLoaderService.LoadScene(SceneName.MainMenu);

        [Button(25), DisableInEditorMode]
        private void DebugLoadBattle() =>
            _scenesLoaderService.LoadScene(SceneName.Gameplay);
    }
}