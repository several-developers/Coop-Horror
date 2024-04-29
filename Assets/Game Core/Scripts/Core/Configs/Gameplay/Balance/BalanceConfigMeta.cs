using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.Balance
{
    public class BalanceConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0), SuffixLabel("seconds", overlay: true)]
        private float _gameRestartDelay = 5f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float GameRestartDelay => _gameRestartDelay;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}