using GameCore.Enums.Global;
using GameCore.Infrastructure.Configs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Global.Game
{
    public class GameConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [TitleGroup(Constants.Settings)]
        [SerializeField]
        private LayerMask _noiseLayers;
        
        [BoxGroup(EditorOnly), SerializeField]
        private bool _useStartScene;

        [BoxGroup(EditorOnly), SerializeField, ShowIf(nameof(_useStartScene))]
        private bool _forceLoadBootstrapScene;

        [BoxGroup(EditorOnly), SerializeField, ShowIf(nameof(_useStartScene))]
        private SceneName _startScene;

        // PROPERTIES: ----------------------------------------------------------------------------

        public LayerMask NoiseLayers => _noiseLayers;
        
        public bool UseStartScene => _useStartScene;
        public bool ForceLoadBootstrapScene => _forceLoadBootstrapScene;
        public SceneName StartScene => _startScene;

        private const string EditorOnly = Constants.Settings + "/Editor Only";

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GlobalConfigsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.Global;
    }
}
