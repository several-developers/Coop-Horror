﻿using GameCore.Gameplay.Managers.Chat;
using GameCore.Gameplay.GameManagement;
using GameCore.Gameplay.GameTimeManagement;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Network.DynamicPrefabs;
using GameCore.Gameplay.Systems.Noise;
using GameCore.Gameplay.Systems.Quests;
using GameCore.InfrastructureTools.Configs;
using GameCore.Observers.Gameplay.Game;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Infrastructure.Configs.Gameplay.PrefabsList
{
    public class PrefabsListConfigMeta : ConfigMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
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
        
        [SerializeField, Required]
        private DynamicPrefabsLoader _dynamicPrefabsLoader;

        // PROPERTIES: ----------------------------------------------------------------------------

        public PlayerSpawner PlayerSpawner => _playerSpawner;
        public GameTimeManager GameTimeManager => _gameTimeManager;
        public QuestsManager QuestsManager => _questsManager;
        public GameManager GameManager => _gameManager;
        public ChatManager ChatManager => _chatManager;
        public GameObserverNetwork GameObserver => _gameObserver;
        public NoiseManager NoiseManager => _noiseManager;
        public DynamicPrefabsLoader DynamicPrefabsLoader => _dynamicPrefabsLoader;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsListsCategory;
        
        public override ConfigScope GetConfigScope() =>
            ConfigScope.GameplayScene;
    }
}