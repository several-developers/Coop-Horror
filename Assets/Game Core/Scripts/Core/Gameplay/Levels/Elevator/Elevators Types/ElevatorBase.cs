using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels.Elevator
{
    public abstract class ElevatorBase : MonoBehaviour
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
        private RpcCaller _rpcCaller;
        private bool _isOpen;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent += OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent += OnFloorChanged;
        }

        protected virtual void Start()
        {
            _rpcCaller = RpcCaller.Get();
            
            _rpcCaller.OnOpenElevatorEvent += OnOpenElevator;
        }

        private void OnDestroy()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent -= OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent -= OnFloorChanged;

            _rpcCaller.OnOpenElevatorEvent -= OnOpenElevator;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetElevatorFloor(Floor floor) =>
            _floor = floor;

        public Floor GetElevatorFloor() => _floor;

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
            
            OpenElevator();
        }

        private void OnOpenElevator(Floor floor)
        {
            bool isSameFloor = floor == _floor;

            if (!isSameFloor)
                return;
            
            OpenElevator();
        }
    }
}