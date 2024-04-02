using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class CallButton : MonoBehaviour, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;
        
        [SerializeField, Required]
        private ElevatorBase _elevator;

        [SerializeField, Required]
        private ControlPanel _controlPanel;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private RpcCaller _rpcCaller;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent += OnElevatorsStopped;
            
            _animationObserver.OnEnabledEvent += OnEnabledEvent;
        }

        private void Start() =>
            _rpcCaller = RpcCaller.Get();

        private void OnDestroy()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent -= OnElevatorsStopped;
            
            _animationObserver.OnEnabledEvent -= OnEnabledEvent;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Interact()
        {
            if (IsElevatorMoving())
                return;
            
            ToggleInteract(canInteract: false);
            PlayAnimation();
            HandleClick();
        }

        public void ToggleInteract(bool canInteract)
        {
            _canInteract = canInteract;
            OnInteractionStateChangedEvent?.Invoke();
        }

        public InteractionType GetInteractionType() =>
            InteractionType.ElevatorCallButton;

        public bool CanInteract() =>
            _canInteract && !IsElevatorMoving();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        private void HandleClick()
        {
            Floor floor = _elevator.GetElevatorFloor();
            Floor currentFloor = _elevatorsManagerDecorator.GetCurrentFloor();
            bool isElevatorMoving = IsElevatorMoving();
            bool openElevator = !isElevatorMoving && currentFloor == floor;

            if (openElevator)
                OpenElevator(floor);
            else
                StartElevator(floor);
        }

        private void OpenElevator(Floor floor) =>
            _rpcCaller.OpenElevator(floor);

        private void StartElevator(Floor floor) =>
            _controlPanel.StartElevator(floor);

        private bool IsElevatorMoving() =>
            _elevatorsManagerDecorator.IsElevatorMoving();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnEnabledEvent()
        {
            bool isElevatorMoving = IsElevatorMoving();

            if (isElevatorMoving)
                return;
            
            ToggleInteract(canInteract: true);
        }
        
        private void OnElevatorsStopped(Floor floor) => ToggleInteract(canInteract: true);
    }
}