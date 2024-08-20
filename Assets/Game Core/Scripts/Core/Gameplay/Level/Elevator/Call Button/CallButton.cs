using System;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class CallButton : SoundProducerMonoBehaviour<ElevatorBase.SFXType>, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IElevatorsManagerDecorator elevatorsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider)
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _elevatorConfig = gameplayConfigsProvider.GetElevatorConfig();
            _soundReproducer = new ElevatorSoundReproducer(soundProducer: this, _elevatorConfig);
        }

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
        private ElevatorConfigMeta _elevatorConfig;
        private ElevatorSoundReproducer _soundReproducer;
        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent += OnElevatorsStopped;

            _animationObserver.OnEnabledEvent += OnEnabledEvent;
        }
        
        private void OnDestroy()
        {
            _elevatorsManagerDecorator.OnElevatorStoppedEvent -= OnElevatorsStopped;

            _animationObserver.OnEnabledEvent -= OnEnabledEvent;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
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

            PlayButtonPushSound();
        }

        private void OpenElevator(Floor floor) =>
            _elevatorsManagerDecorator.OpenElevator(floor);

        private void StartElevator(Floor floor) =>
            _controlPanel.StartElevator(floor);

        private void PlayButtonPushSound() =>
            _soundReproducer.PlaySound(ElevatorBase.SFXType.ButtonPush);

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