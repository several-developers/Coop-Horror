using GameCore.Enums;
using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Game
{
    public class GameConfigMeta : EditorMeta
    {
        [TitleGroup(Constants.Settings)]
        [BoxGroup(EditorOnly), SerializeField]
        private bool _useStartScene;

        [BoxGroup(EditorOnly), SerializeField, ShowIf(nameof(_useStartScene))]
        private bool _forceLoadBootstrapScene;

        [BoxGroup(EditorOnly), SerializeField, ShowIf(nameof(_useStartScene))]
        private SceneName _startScene;

        public bool UseStartScene => _useStartScene;
        public bool ForceLoadBootstrapScene => _forceLoadBootstrapScene;
        public SceneName StartScene => _startScene;

        private const string EditorOnly = Constants.Settings + "/Editor Only";

        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsCategory;
    }
}
