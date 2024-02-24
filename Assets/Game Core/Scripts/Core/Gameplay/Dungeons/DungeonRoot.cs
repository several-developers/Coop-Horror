using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Levels;
using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.Dungeons;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonRoot : MonoBehaviour, IDungeonCompleteReceiver
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelManager levelManager, IDungeonsObserver dungeonsObserver)
        {
            _levelManager = levelManager;
            _dungeonsObserver = dungeonsObserver;
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private Floor _floor;
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private DungeonReferences _dungeonReferences;
        
        // FIELDS: --------------------------------------------------------------------------------
        
        private ILevelManager _levelManager;
        private IDungeonsObserver _dungeonsObserver;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Floor Floor => _floor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void AddFireExitToLevelManager(FireExit fireExit)
        {
            bool isInStairsLocation = fireExit.IsInStairsLocation;
            
            if (isInStairsLocation)
                _levelManager.AddStairsFireExit(_floor, fireExit);
            else
                _levelManager.AddOtherFireExit(_floor, fireExit);
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDungeonComplete(Dungeon dungeon)
        {
            ElevatorBase dungeonElevator = _dungeonReferences.GetElevator();
            dungeonElevator.ChangeElevatorFloor(_floor);
            
            _dungeonsObserver.SendDungeonGenerationCompleted(_floor, _dungeonReferences);
        }
    }
}