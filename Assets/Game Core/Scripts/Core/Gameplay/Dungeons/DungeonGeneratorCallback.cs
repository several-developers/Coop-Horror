using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.Dungeons;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonGeneratorCallback : MonoBehaviour, IDungeonCompleteReceiver
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDungeonsObserver dungeonsObserver) =>
            _dungeonsObserver = dungeonsObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private ElevatorFloor _elevatorFloor;

        [Title(Constants.References)]
        [SerializeField, Required]
        private DungeonReferences _dungeonReferences;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IDungeonsObserver _dungeonsObserver;
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDungeonComplete(Dungeon dungeon)
        {
            ElevatorBase dungeonElevator = _dungeonReferences.GetElevator();
            dungeonElevator.ChangeElevatorFloor(_elevatorFloor);
            
            _dungeonsObserver.SendDungeonGenerationCompleted(_elevatorFloor, _dungeonReferences);
        }
    }
}