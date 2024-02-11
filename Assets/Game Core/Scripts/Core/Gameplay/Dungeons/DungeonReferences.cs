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
        private DungeonElevator _dungeonElevator;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AddDungeonElevator(DungeonElevator dungeonElevator) =>
            _dungeonElevator = dungeonElevator;

        public DungeonElevator GetDungeonElevator() => _dungeonElevator;
    }
}