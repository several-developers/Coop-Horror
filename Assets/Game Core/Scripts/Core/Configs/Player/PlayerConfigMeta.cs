using MetaEditor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Core.Configs.Player
{
    public class PlayerConfigMeta : EditorMeta
    {
        [Title(Constants.Settings)]
        [SerializeField, Min(0)]
        private float _movementSpeed = 5f;

        [SerializeField, Min(0)]
        private float _crouchSpeed = 2.5f;

        [SerializeField, Min(0)]
        private float _jumpHeight = 15f;

        [Title(Constants.References)]
        [SerializeField, Required]
        private GameObject _playerPrefab;

        public float MovementSpeed => _movementSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float JumpHeight => _jumpHeight;
        public GameObject PlayerPrefab => _playerPrefab;

        public override string GetMetaCategory() =>
            EditorConstants.PlayerConfigsCategory;
    }
}
