using CustomEditors;
using GameCore.Gameplay.ChatManagement;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Quests;
using GameCore.Observers.Gameplay.Game;
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
        private GameTimeManager _gameTimeManager;

        [SerializeField, Required]
        private QuestsManager _questsManager;
        
        [SerializeField, Required]
        private GameManager _gameManager;
        
        [SerializeField, Required]
        private ChatManager _chatManager;

        [SerializeField, Required]
        private GameObserverNetwork _gameObserver;
        
        [SerializeField, Required]
        private NoiseManager _noiseManager;

        // PROPERTIES: ----------------------------------------------------------------------------

        public ElevatorsManager ElevatorsManager => _elevatorsManager;
        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public QuestsManager QuestsManager => _questsManager;
        public GameManager GameManager => _gameManager;
        public ChatManager ChatManager => _chatManager;
        public GameObserverNetwork GameObserver => _gameObserver;
        public NoiseManager NoiseManager => _noiseManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}