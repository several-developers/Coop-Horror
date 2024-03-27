using GameCore.Gameplay.Levels.Elevator;
using GameCore.Observers.Gameplay.Dungeons;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonElevatorRegistrar : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDungeonsObserver dungeonsObserver) =>
            _dungeonsObserver = dungeonsObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private DungeonElevator _dungeonElevator;

        // FIELDS: --------------------------------------------------------------------------------
        
        private IDungeonsObserver _dungeonsObserver;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => RegisterDungeon();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterDungeon()
        {
            bool isDungeonRootFound = transform.parent.TryGetComponent(out DungeonRoot dungeonRoot);

            if (!isDungeonRootFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }
            
            _dungeonElevator.SetElevatorFloor(dungeonRoot.Floor);
            _dungeonsObserver.RegisterElevator(_dungeonElevator);
        }
    }
}