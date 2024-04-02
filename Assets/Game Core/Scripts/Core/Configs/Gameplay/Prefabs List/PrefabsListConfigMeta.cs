using CustomEditors;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.PrefabsList
{
    public class PrefabsListConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private ElevatorsManager _elevatorsManager;

        [SerializeField, Required]
        private PlayerSpawner _playerSpawner;
        
        [SerializeField, Required]
        private RpcCaller _rpcCaller;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;
        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public RpcCaller RpcCaller => _rpcCaller;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}