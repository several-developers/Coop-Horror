using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public abstract class ElevatorBase : NetcodeBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;
        
        // FIELDS: --------------------------------------------------------------------------------

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent += OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent += OnFloorChanged;
            _elevatorsManagerDecorator.OnElevatorOpenedEvent += OnElevatorOpened;
        }

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
        
        protected void SetElevatorFloor(Floor floor) =>
            _floor = floor;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OpenElevator()
        {
            if (_isOpen)
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
            
            CloseElevator();
        }

        private void OnFloorChanged(ElevatorStaticData data)
        {
            bool isTargetFloor = data.IsTargetFloor;
            
            if (!isTargetFloor)
                return;
            
            Floor currentFloor = data.CurrentFloor;
            bool isSameFloor = currentFloor == _floor;

            if (!isSameFloor)
                return;

            if (_isOpen)
                return;
            
            OpenElevator();
        }

        private void OnElevatorOpened(Floor floor)
        {
            bool isSameFloor = floor == _floor;

            if (!isSameFloor)
                return;

            if (_isOpen)
                return;
            
            OpenElevator();
        }
    }
}