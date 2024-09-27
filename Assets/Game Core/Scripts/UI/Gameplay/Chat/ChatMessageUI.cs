using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.UI.Gameplay.Chat
{
    public class ChatMessageUI : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshProUGUI _textTMP;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetText(string text) =>
            _textTMP.text = text;
    }
}