using System;
using GameCore.Infrastructure.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Level.Elevator;
using GameCore.Gameplay.Interactable;
using GameCore.Gameplay.Other;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class CallButton : SoundProducerMonoBehaviour<ElevatorEntity.SFXType>, IInteractable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IGameplayConfigsProvider gameplayConfigsProvider)
        {
            var elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
            SoundReproducer = new ElevatorSoundReproducer(soundProducer: this, elevatorConfig);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _buttonFloor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        [SerializeField, Required]
        private AnimationObserver _animationObserver;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnInteractionStateChangedEvent;

        private bool _canInteract = true;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            _animationObserver.OnEnabledEvent += OnButtonEnabled;

        private void OnDestroy() =>
            _animationObserver.OnEnabledEvent -= OnButtonEnabled;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void InteractionStarted(IEntity entity = null)
        {
        }

        public void InteractionEnded(IEntity entity = null)
        {
        }

        public void Interact(IEntity entity = null)
        {
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

        public bool CanInteract() => _canInteract;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PlayAnimation() =>
            _animator.SetTrigger(id: AnimatorHashes.Trigger);

        private void HandleClick()
        {
            StartOrOpenElevator();
            PlayButtonPushSound();
        }

        private void StartOrOpenElevator()
        {
            bool canOpenElevator = CanOpenElevator();
            
            if (canOpenElevator)
                OpenElevator();
            else
                StartElevator();
        }

        private static void OpenElevator()
        {
            ElevatorEntity elevatorEntity = GetElevatorEntity();
            elevatorEntity.Open();
        }
        
        private void StartElevator()
        {
            ElevatorEntity elevatorEntity = GetElevatorEntity();
            elevatorEntity.StartElevator(_buttonFloor);
        }

        private void PlayButtonPushSound() => PlaySound(ElevatorEntity.SFXType.ButtonPush).Forget();

        private static ElevatorEntity GetElevatorEntity() =>
            ElevatorEntity.Get();

        private bool CanOpenElevator()
        {
            ElevatorEntity elevatorEntity = GetElevatorEntity();
            ElevatorEntity.ElevatorState elevatorState = elevatorEntity.GetElevatorState();
            bool isIdleState = elevatorState == ElevatorEntity.ElevatorState.Idle;

            if (!isIdleState)
                return false;

            Floor currentFloor = elevatorEntity.GetCurrentElevatorFloor();
            bool isFloorsMatches = currentFloor == _buttonFloor;
            return isFloorsMatches;
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnButtonEnabled() => ToggleInteract(canInteract: true);
    }
}