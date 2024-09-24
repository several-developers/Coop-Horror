using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Level.Elevator;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace GameCore.Gameplay.Level.Elevator
{
    public abstract class FloorDisplayBase : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private TextMeshPro _floorNumberTMP;

        // FIELDS: --------------------------------------------------------------------------------

        protected ElevatorEntity ElevatorEntity;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Start() =>
            ElevatorEntity = ElevatorEntity.Get();

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected abstract void UpdateFloorNumber();
        
        protected void UpdateFloorNumber(Floor floor)
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
    }
}