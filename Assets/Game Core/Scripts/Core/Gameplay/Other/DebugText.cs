using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Other
{
    public class DebugText : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        // FIELDS: --------------------------------------------------------------------------------

        public static DebugText Instance;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => Instance = this;

        public void SetText(string text) =>
            _textTMP.text = text;
    }
}