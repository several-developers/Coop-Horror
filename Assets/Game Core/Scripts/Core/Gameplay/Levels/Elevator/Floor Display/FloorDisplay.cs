using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class FloorDisplay : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshPro _floorNumberTMP;
        
        [SerializeField, Required]
        private Transform _moveDirectionTransform;

        // FIELDS: --------------------------------------------------------------------------------

        private ElevatorsManager _elevatorsManager;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            UpdateFloorNumber(Floor.Surface);
            
            _elevatorsManager = ElevatorsManager.Get();
            
            _elevatorsManager.OnElevatorStartedEvent += OnElevatorsStarted;
            _elevatorsManager.OnFloorChangedEvent += OnFloorChanged;
        }

        private void OnDestroy()
        {
            _elevatorsManager.OnElevatorStartedEvent -= OnElevatorsStarted;
            _elevatorsManager.OnFloorChangedEvent -= OnFloorChanged;
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