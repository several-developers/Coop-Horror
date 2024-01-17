﻿using GameCore.Gameplay.Entities.Player;
using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Core.Configs.Player
{
    public class PlayerConfigMeta : EditorMeta
    {
        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _health = 100f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab; // TEMP

        public PlayerEntity PlayerPrefab => _playerPrefab;

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}
