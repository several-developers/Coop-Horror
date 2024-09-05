using GameCore.Observers.Global.StateMachine;
using GameCore.StateMachine;
using GameCore.UI.Global.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace GameCore.UI.Gameplay.DeveloperMenu.CheatButtons
{
    public class FinishLevelButton : BaseButton
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameStateMachine gameStateMachine, IGameStateMachineObserver gameStateMachineObserver)
        {
            _gameStateMachine = gameStateMachine;
            _gameStateMachineObserver = gameStateMachineObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Button _button;

        // FIELDS: --------------------------------------------------------------------------------

        private IGameStateMachine _gameStateMachine;
        private IGameStateMachineObserver _gameStateMachineObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            _gameStateMachineObserver.OnStateChangedEvent += OnStateChanged;
        }

        private void Start() => CheckButtonState();

        private void OnDestroy() =>
            _gameStateMachineObserver.OnStateChangedEvent -= OnStateChanged;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void ClickLogic()
        {
            EnterGameOverState();
            CheckButtonState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckButtonState()
        {
            bool isStateExists = _gameStateMachine.TryGetCurrentState(out IState state);

            if (!isStateExists)
                return;

            //bool isInteractable = state is GameplaySceneState;
            //_button.interactable = isInteractable;
        }

        private void EnterGameOverState()
        {
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnStateChanged() => CheckButtonState();
    }
}