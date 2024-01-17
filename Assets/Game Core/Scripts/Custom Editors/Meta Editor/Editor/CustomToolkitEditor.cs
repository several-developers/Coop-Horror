#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CustomEditors
{
    public class CustomToolkitEditor
    {
        // FIELDS: --------------------------------------------------------------------------------

        private const string ScenesMenuItem = "🕹 Horror/💾 Scenes/";
        private const string ScenesPath = "Assets/Game Core/Scenes/";

        private const string BootstrapSceneMenuItem = ScenesMenuItem + "🚀 Bootstrap";

        //private const string LoginSceneMenuItem = ScenesMenuItem + "🗝 Login";
        //private const string TitleSceneMenuItem = ScenesMenuItem + "✨ Title";
        private const string MainMenuSceneMenuItem = ScenesMenuItem + "🌐 Main Menu";

        private const string GameplaySceneMenuItem = ScenesMenuItem + "⚔ Gameplay";
        //private const string MultiplayerTestSceneMenuItem = ScenesMenuItem + "⚔ Multiplayer Test";
        //private const string PrototypesSceneMenuItem = ScenesMenuItem + "⏳ Prototypes";

        private const string BootstrapScenePath = ScenesPath + "Bootstrap.unity";

        //private const string LoginScenePath = ScenesPath + "PixelBattleLogin.unity";
        //private const string TitleScenePath = ScenesPath + "TitleScreen.unity";
        private const string MainMenuScenePath = ScenesPath + "MainMenu.unity";

        private const string GameplayScenePath = ScenesPath + "Gameplay.unity";
        //private const string MultiplayerTestScenePath = ScenesPath + "MultiplayerTest.unity";
        //private const string PrototypesScenePath = ScenesPath + "Prototypes.unity";

        // PRIVATE METHODS: -----------------------------------------------------------------------

        [MenuItem(BootstrapSceneMenuItem)]
        private static void LoadBootstrapScene() =>
            OpenScene(BootstrapScenePath);

        // [MenuItem(LoginSceneMenuItem)]
        // private static void LoadLoginScene() =>
        //     OpenScene(LoginScenePath);
        //
        // [MenuItem(TitleSceneMenuItem)]
        // private static void LoadTitleScene() =>
        //     OpenScene(TitleScenePath);

        [MenuItem(MainMenuSceneMenuItem)]
        private static void LoadMainMenuScene() =>
            OpenScene(MainMenuScenePath);

        [MenuItem(GameplaySceneMenuItem)]
        private static void LoadGameScene() =>
            OpenScene(GameplayScenePath);

        // [MenuItem(MultiplayerTestSceneMenuItem)]
        // private static void LoadMultiplayerTestScene() =>
        //     OpenScene(MultiplayerTestScenePath);
        //
        // [MenuItem(PrototypesSceneMenuItem)]
        // private static void LoadPrototypesScene() =>
        //     OpenScene(PrototypesScenePath);

        private static void OpenScene(string path)
        {
            if (!Application.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }
}
#endif