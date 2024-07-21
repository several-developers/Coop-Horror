#if UNITY_EDITOR
using GameCore;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CustomEditors
{
    public static class CustomToolkitEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const string ScenesMenuItem = EditorConstants.GameMenuName + "/💾 Scenes/";
        private const string LocationsScenesMenuItem = ScenesMenuItem + "🌍 Locations/";
        private const string ScenesPath = "Assets/Game Core/Scenes/";
        private const string LocationsScenesPath = ScenesPath + "Locations/";

        private const string ScrapyardLocationSceneMenuItem = LocationsScenesMenuItem + "🗑️ Scrapyard";
        private const string DesertLocationSceneMenuItem = LocationsScenesMenuItem + "🌵 Desert";
        private const string ForestLocationSceneMenuItem = LocationsScenesMenuItem + "🌲 Forest";

        private const string BootstrapSceneMenuItem = ScenesMenuItem + "🚀 Bootstrap";
        //private const string LoginSceneMenuItem = ScenesMenuItem + "🗝 Login";
        //private const string TitleSceneMenuItem = ScenesMenuItem + "✨ Title";
        private const string MainMenuSceneMenuItem = ScenesMenuItem + "🌐 Main Menu";

        private const string GameplaySceneMenuItem = ScenesMenuItem + "⚔ Gameplay";
        private const string QuickStartMenuItem = EditorConstants.GameMenuName + "/⚡ Quick Start";
        //private const string MultiplayerTestSceneMenuItem = ScenesMenuItem + "⚔ Multiplayer Test";
        //private const string PrototypesSceneMenuItem = ScenesMenuItem + "⏳ Prototypes";

        private const string BootstrapScenePath = ScenesPath + "Bootstrap.unity";

        //private const string LoginScenePath = ScenesPath + "PixelBattleLogin.unity";
        //private const string TitleScenePath = ScenesPath + "TitleScreen.unity";
        private const string MainMenuScenePath = ScenesPath + "MainMenu.unity";
        private const string GameplayScenePath = ScenesPath + "Gameplay.unity";

        private const string ScrapyardLocationScenePath = LocationsScenesPath + "Scrapyard.unity";
        private const string DesertLocationScenePath = LocationsScenesPath + "Desert.unity";
        private const string ForestLocationScenePath = LocationsScenesPath + "Forest.unity";

        private const string RestoreSceneKey = "RestoreScene";
        private const string ScenePathKey = "ScenePath";

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [InitializeOnEnterPlayMode]
        private static void Init() =>
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        [MenuItem(ScrapyardLocationSceneMenuItem)]
        private static void LoadScrapyardLocationScene() =>
            OpenScene(ScrapyardLocationScenePath);

        [MenuItem(DesertLocationSceneMenuItem)]
        private static void LoadDesertLocationScene() =>
            OpenScene(DesertLocationScenePath);
        
        [MenuItem(ForestLocationSceneMenuItem)]
        private static void LoadForestLocationScene() =>
            OpenScene(ForestLocationScenePath);
        
        [MenuItem(BootstrapSceneMenuItem)]
        private static void LoadBootstrapScene() =>
            OpenScene(BootstrapScenePath);

        [MenuItem(MainMenuSceneMenuItem)]
        private static void LoadMainMenuScene() =>
            OpenScene(MainMenuScenePath);

        [MenuItem(GameplaySceneMenuItem)]
        private static void LoadGameScene() =>
            OpenScene(GameplayScenePath);

        [MenuItem(QuickStartMenuItem)]
        private static void QuickStart()
        {
            if (Application.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string scenePath = EditorSceneManager.GetActiveScene().path;
            SaveScenePath(scenePath);
            
            EditorSceneManager.OpenScene(BootstrapScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();

            EnableRestoreScene();
        }
        
        private static void OpenScene(string path)
        {
            bool canOpenScene = !Application.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            
            if (!canOpenScene)
                return;
            
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        private static void EnableRestoreScene() =>
            PlayerPrefs.SetInt(RestoreSceneKey, 1);

        private static void DisableRestoreScene() =>
            PlayerPrefs.SetInt(RestoreSceneKey, 0);

        private static void SaveScenePath(string path) =>
            PlayerPrefs.SetString(ScenePathKey, path);

        private static bool TryGetScenePath(out string path)
        {
            path = PlayerPrefs.GetString(ScenePathKey);
            bool isPathValid = !string.IsNullOrEmpty(path);
            return isPathValid;
        }

        private static bool ShouldRestoreScene() =>
            PlayerPrefs.GetInt(RestoreSceneKey, defaultValue: 0) == 1;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;

            if (!ShouldRestoreScene())
                return;

            bool isPathValid = TryGetScenePath(out string path);

            if (!isPathValid)
                return;

            DisableRestoreScene();
            OpenScene(path);
        }
    }
}
#endif