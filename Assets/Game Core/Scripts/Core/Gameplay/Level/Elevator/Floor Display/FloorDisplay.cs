using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Level.Elevator
{
    public class FloorDisplay : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IElevatorsManagerDecorator elevatorsManagerDecorator) =>
            _elevatorsManagerDecorator = elevatorsManagerDecorator;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshPro _floorNumberTMP;
        
        [SerializeField, Required]
        private Transform _moveDirectionTransform;

        // FIELDS: --------------------------------------------------------------------------------

        private IElevatorsManagerDecorator _elevatorsManagerDecorator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent += OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent += OnFloorChanged;
        }

        private void Start() => UpdateFloorNumber(Floor.Surface);

        private void OnDestroy()
        {
            _elevatorsManagerDecorator.OnElevatorStartedEvent -= OnElevatorsStarted;
            _elevatorsManagerDecorator.OnFloorChangedEvent -= OnFloorChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void UpdateDisplayInfo(ElevatorStaticData data)
        {
            Floor currentFloor = data.CurrentFloor;
            bool isTargetFloor = data.IsTargetFloor;
            
            _moveDirectionTransform.gameObject.SetActive(!isTargetFloor);
            
            UpdateFloorNumber(currentFloor);
            UpdateMoveDirection(data.IsMovingUp);
        }

        private void UpdateFloorNumber(Floor floor)
        {
            string text = floor switch
            {
                Floor.One => "1",
                Floor.Two => "2",
                Floor.Three => "3",
                _ => "~"
            };

            _floorNumberTMP.text = text;
        }

        private void UpdateMoveDirection(bool isMovingUp)
        {
            Vector3 rotation = isMovingUp ? Vector3.zero : new Vector3(x: 0f, y: 0f, z: 180f);
            _moveDirectionTransform.rotation = Quaternion.Euler(rotation);
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnElevatorsStarted(ElevatorStaticData data) => UpdateDisplayInfo(data);

        private void OnFloorChanged(ElevatorStaticData data) => UpdateDisplayInfo(data);
    }
}