using Cysharp.Threading.Tasks;
using GameCore.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Systems.SoundReproducer;
using GameCore.Infrastructure.Providers.Gameplay.GameplayConfigs;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    [GenerateSerializationForType(typeof(SFXType))]
    public abstract class ElevatorBase : SoundProducerEntity<ElevatorBase.SFXType>
    {
        public enum SFXType
        {
            // _ = 0,
            DoorOpening = 1,
            DoorClosing = 2,
            FloorChange = 3,
            ButtonPush = 4
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(
            IElevatorsManagerDecorator elevatorsManagerDecorator,
            IGameplayConfigsProvider gameplayConfigsProvider
        )
        {
            _elevatorsManagerDecorator = elevatorsManagerDecorator;
            _elevatorConfig = gameplayConfigsProvider.GetConfig<ElevatorConfigMeta>();
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private ElevatorConfigMeta _elevatorConfig;

        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent += OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent += OnFloorChanged;
            _elevatorsManagerDecorator.OnElevatorOpenedEvent += OnElevatorOpened;
        }

        protected override void StartServerOnly() => TrySpawnNetworkObject();

        public override void OnDestroy()
        {
            base.OnDestroy();

            _elevatorsManagerDecorator.OnElevatorStartedEvent -= OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent -= OnFloorChanged;
            _elevatorsManagerDecorator.OnElevatorOpenedEvent -= OnElevatorOpened;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Floor GetElevatorFloor() => _floor;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitAll() =>
            SoundReproducer = new ElevatorSoundReproducer(soundProducer: this, _elevatorConfig);

        protected void SetElevatorFloor(Floor floor) =>
            _floor = floor;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void TrySpawnNetworkObject()
        {
            if (IsSpawned)
                return;
            
            NetworkObject.Spawn();
        }
        
        private async UniTaskVoid OpenElevator()
        {
            if (_isOpen)
                return;

            float delayInSeconds = _elevatorConfig.DoorOpenDelay;
            int delay = delayInSeconds.ConvertToMilliseconds();

            bool isCanceled = await UniTask
                .Delay(delay, cancellationToken: this.GetCancellationTokenOnDestroy())
                .SuppressCancellationThrow();

            if (isCanceled)
                return;

            _isOpen = true;
            _animator.SetTrigger(id: AnimatorHashes.Open);
        }

        private void CloseElevator()
        {
            _isOpen = false;
            _animator.SetTrigger(id: AnimatorHashes.Close);
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnElevatorsStarted(ElevatorStaticData data)
        {
            bool isSameFloor = data.CurrentFloor == _floor;

            if (!isSameFloor)
                return;

            if (!_isOpen)
                return;
            
            if (IsServerOnly)
                PlaySound(SFXType.DoorClosing).Forget();

            CloseElevator();
        }

        private void OnFloorChanged(ElevatorStaticData data)
        {
            bool isTargetFloor = data.IsTargetFloor;

            if (!isTargetFloor)
            {
                if (IsServerOnly && data.CurrentFloor == _floor)
                    PlaySound(SFXType.FloorChange).Forget();
                
                return;
            }

            Floor currentFloor = data.CurrentFloor;
            bool isSameFloor = currentFloor == _floor;

            if (!isSameFloor)
                return;

            if (_isOpen)
                return;
            
            if (IsServerOnly)
                PlaySound(SFXType.DoorOpening).Forget();

            OpenElevator().Forget();
        }

        private void OnElevatorOpened(Floor floor)
        {
            bool isSameFloor = floor == _floor;

            if (!isSameFloor)
                return;

            if (_isOpen)
                return;

            if (IsServerOnly)
                PlaySound(SFXType.DoorOpening).Forget();
            
            OpenElevator().Forget();
        }
    }
}