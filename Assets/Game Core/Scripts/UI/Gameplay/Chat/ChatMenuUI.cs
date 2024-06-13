using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Gameplay.ChatManagement;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.UIManager;
using GameCore.UI.Global.MenuView;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.Chat
{
    public class ChatMenuUI : MenuView
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IChatManagerDecorator chatManagerDecorator, IUIManagerObserver uiManagerObserver)
        {
            _chatManagerDecorator = chatManagerDecorator;
            _uiManagerObserver = uiManagerObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _hideChatDelay = 4f;

        [SerializeField, Min(0f)]
        private float _fadeOutDuration = 0.4f;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private TMP_InputField _chatIF;

        [SerializeField, Required]
        private TextMeshProUGUI _timerHintTMP;
        
        [SerializeField, Required]
        private ChatMessageUI _chatMessagePrefab;

        [SerializeField, Required]
        private Transform _messagesContainer;

        [SerializeField, Required]
        private LayoutGroup _layoutGroup;

        [SerializeField, Required]
        private ContentSizeFitter _contentSizeFitter;

        [SerializeField, Required]
        private ScrollRect _chatScrollRect;

        [SerializeField, Required]
        private GameObject _scrollBar;

        // FIELDS: --------------------------------------------------------------------------------

        private IChatManagerDecorator _chatManagerDecorator;
        private IUIManagerObserver _uiManagerObserver;
        private LayoutFixHelper _layoutFixHelper;

        private Coroutine _hideTimerCO;
        private Tweener _fadeTN;
        private bool _isCompletelyHidden = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();

            _layoutFixHelper = new LayoutFixHelper(coroutineRunner: this, _layoutGroup, _contentSizeFitter);
            _layoutGroup.enabled = false;
            _contentSizeFitter.enabled = false;

            _chatManagerDecorator.OnChatMessageReceivedEvent += OnChatMessageReceived;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _chatManagerDecorator.OnChatMessageReceivedEvent -= OnChatMessageReceived;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ActivateChat() => ToggleChatMenuState(isEnabled: true);

        public void TrySendChatMessage()
        {
            string text = _chatIF.text;
            bool isValid = !string.IsNullOrWhiteSpace(text);

            if (!isValid)
                return;

            bool canSendMessage = _chatManagerDecorator.CanSendMessage();

            if (!canSendMessage)
            {
                if (!_timerHintTMP.enabled)
                    ToggleTimerHintState(isEnabled: true);
                
                _chatIF.ActivateInputField();
                _chatIF.Select();
                return;
            }
            
            SendChatMessage();
            Hide();
        }

        public override void Show()
        {
            base.Show();
            
            if (_isCompletelyHidden)
                ScrollChatToBottom();
            
            VisibilityState(show: true);
            StopHideTimer();
            
            _chatManagerDecorator.OnTimerTickEvent += OnTimerTick;
        }

        public override void Hide()
        {
            base.Hide();
            ToggleChatMenuState(isEnabled: false);
            ToggleTimerHintState(isEnabled: false);
            StopHideTimer();
            StartHideTimer();
            
            _chatManagerDecorator.OnTimerTickEvent -= OnTimerTick;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SendChatMessage()
        {
            ulong clientID = NetworkHorror.ClientID;
            string text = _chatIF.text;
            string message = $"Player #{clientID}:  > {text}";

            _chatIF.text = string.Empty;
            _chatManagerDecorator.SendChatMessage(message);
        }
        
        private void CreateChatMessage(string message)
        {
            ChatMessageUI chatMessage = Instantiate(_chatMessagePrefab, _messagesContainer);
            chatMessage.SetText(message);
        }

        private void ToggleChatMenuState(bool isEnabled)
        {
            _chatIF.gameObject.SetActive(isEnabled);
            _chatScrollRect.enabled = isEnabled;
            _scrollBar.SetActive(isEnabled);
        }

        private void VisibilityState(bool show)
        {
            _isCompletelyHidden = false;
            
            float endValue = show ? 1f : 0f;
            float duration = show ? FadeTime : _fadeOutDuration;
            
            _fadeTN.Kill();

            _fadeTN = TargetCG
                .DOFade(endValue, duration)
                .OnComplete(() =>
                {
                    if (!show)
                        _isCompletelyHidden = true;
                });
        }

        private void ScrollChatToBottom()
        {
            _layoutGroup.enabled = true;
            _contentSizeFitter.enabled = true;
            
            Canvas.ForceUpdateCanvases();
            
            _chatScrollRect.verticalNormalizedPosition = 0f;
            
            _layoutFixHelper.FixLayout();
        }

        private void ToggleTimerHintState(bool isEnabled) =>
            _timerHintTMP.enabled = isEnabled;

        private void StartHideTimer()
        {
            IEnumerator routine = HideTimerCO();
            _hideTimerCO = StartCoroutine(routine);
        }

        private void StopHideTimer()
        {
            if (_hideTimerCO == null)
                return;
            
            StopCoroutine(_hideTimerCO);
        }
        
        private IEnumerator HideTimerCO()
        {
            yield return new WaitForSeconds(_hideChatDelay);
            
            VisibilityState(show: false);
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        protected override async void OnShowMenu()
        {
            _uiManagerObserver.MenuShown(menuView: this);

            bool isCanceled = await UniTask
                .DelayFrame(delayFrameCount: 1, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;
            
            _chatIF.ActivateInputField();
            _chatIF.Select();
        }

        protected override void OnHideMenu() =>
            _uiManagerObserver.MenuHidden(menuView: this);
        
        private void OnChatMessageReceived(string message)
        {
            CreateChatMessage(message);
            ScrollChatToBottom();
        }

        private void OnTimerTick(float timeLeft)
        {
            _timerHintTMP.text = $"Wait {timeLeft:F1}s...";

            bool hideTimer = Mathf.Approximately(a: timeLeft, b: 0f);

            if (hideTimer && _timerHintTMP.enabled)
                ToggleTimerHintState(isEnabled: false);
        }
    }
}