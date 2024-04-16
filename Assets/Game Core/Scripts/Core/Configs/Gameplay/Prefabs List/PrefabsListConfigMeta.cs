using CustomEditors;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Delivery;
using GameCore.Gameplay.Levels.Elevator;
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
        private DeliveryManager _deliveryManager;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;
        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public RpcHandler RpcHandler => _rpcHandler;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public QuestsManager QuestsManager => _questsManager;
        public DeliveryManager DeliveryManager => _deliveryManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}