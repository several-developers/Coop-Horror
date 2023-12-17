using System;
using GameCore.Enums;
using GameCore.Infrastructure.Services.Global;
using GameCore.UI.Global.MenuView;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.MainMenu.OfflineMenu
{
    public class OfflineMenuView : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IScenesLoaderService scenesLoaderService) =>
            _scenesLoaderService = scenesLoaderService;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _startGameButton;

        // FIELDS: --------------------------------------------------------------------------------

        private IScenesLoaderService _scenesLoaderService;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);

            DestroyOnHide();
        }

        private void Start() => Show();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void LoadGameplayScene() =>
            _scenesLoaderService.LoadScene(SceneName.Gameplay);

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStartGameButtonClicked() => LoadGameplayScene();
    }
}