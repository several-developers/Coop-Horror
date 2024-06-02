using CustomEditors;
using GameCore.Gameplay.Entities.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Player
{
    public class PlayerConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _health = 100f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private PlayerEntity _playerPrefab; // TEMP

        // PROPERTIES: ----------------------------------------------------------------------------

        public float Health => _health;
        public PlayerEntity PlayerPrefab => _playerPrefab;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}
