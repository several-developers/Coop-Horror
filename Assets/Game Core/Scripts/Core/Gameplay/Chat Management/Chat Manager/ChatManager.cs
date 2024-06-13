using GameCore.Gameplay.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.ChatManagement
{
    public class ChatManager : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IChatManagerDecorator chatManagerDecorator) =>
            _chatManagerDecorator = chatManagerDecorator;

        // FIELDS: --------------------------------------------------------------------------------

        private const float MessagesDelay = 2f;
        
        private IChatManagerDecorator _chatManagerDecorator;
        private float _currentDelayTime;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _chatManagerDecorator.OnSendChatMessageInnerEvent += TrySendMessage;
            _chatManagerDecorator.OnCanSendMessageInnerEvent += IsTimerReady;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _chatManagerDecorator.OnSendChatMessageInnerEvent -= TrySendMessage;
            _chatManagerDecorator.OnCanSendMessageInnerEvent -= IsTimerReady;
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void TickAll()
        {
            if (IsTimerReady())
                return;
            
            float value = _currentDelayTime + Time.deltaTime;
            _currentDelayTime = Mathf.Clamp(value, min: 0f, max: MessagesDelay);

            float timeLeft = MessagesDelay - _currentDelayTime;
            _chatManagerDecorator.SendTimerTick(timeLeft);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrySendMessage(string message)
        {
            bool isValid = !string.IsNullOrWhiteSpace(message);

            if (!isValid)
                return;
            
            if (!IsTimerReady())
                return;

            _currentDelayTime = 0f;
            SendChatMessageServerRpc(message);
        }

        private bool IsTimerReady() =>
            Mathf.Approximately(a: _currentDelayTime, b: MessagesDelay);

        // RPC: -----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SendChatMessageServerRpc(string message) =>
            SendChatMessageClientRpc(message);

        [ClientRpc]
        private void SendChatMessageClientRpc(string message) =>
            _chatManagerDecorator.ChatMessageReceived(message);
    }
}