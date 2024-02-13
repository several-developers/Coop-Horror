using DunGen;
using GameCore.Gameplay.Levels.Elevator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonReferences : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Dungeon _dungeon;

        [SerializeField, ReadOnly]
        private ElevatorBase _elevator;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void SetElevator(ElevatorBase elevator) =>
            _elevator = elevator;

        public ElevatorBase GetElevator() => _elevator;
    }
}