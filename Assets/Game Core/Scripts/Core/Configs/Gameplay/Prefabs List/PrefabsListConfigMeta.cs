using CustomEditors;
using GameCore.Core.Gameplay.GameTimerManagement;
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
        private RpcHandler _rpcHandler;

        [SerializeField, Required]
        private GameTimerManager _gameTimerManager;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;
        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public RpcHandler RpcHandler => _rpcHandler;
        public GameTimerManager GameTimerManager => _gameTimerManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}