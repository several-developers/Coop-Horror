using CustomEditors;
using GameCore.Gameplay.ChatManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Quests;
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
        private GameTimeManager _gameTimeManager;

        [SerializeField, Required]
        private QuestsManager _questsManager;
        
        [SerializeField, Required]
        private GameManager _gameManager;
        
        [SerializeField, Required]
        private ChatManager _chatManager;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;
        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public RpcHandler RpcHandler => _rpcHandler;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public QuestsManager QuestsManager => _questsManager;
        public GameManager GameManager => _gameManager;
        public ChatManager ChatManager => _chatManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}