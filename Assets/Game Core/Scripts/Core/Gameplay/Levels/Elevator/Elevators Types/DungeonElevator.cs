using GameCore.Gameplay.Dungeons;
using GameCore.Observers.Gameplay.LevelManager;
using GameCore.Utilities;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class DungeonElevator : ElevatorBase
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(ILevelProviderObserver levelProviderObserver) =>
            _levelProviderObserver = levelProviderObserver;

        // FIELDS: --------------------------------------------------------------------------------

        private ILevelProviderObserver _levelProviderObserver;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => RegisterDungeon();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RegisterDungeon()
        {
            DungeonRoot dungeonRoot = null;
            Transform parent = transform.parent;
            bool isParentFound = parent != null;
            bool isDungeonRootFound = false;
            int iterations = 0;

            while (isParentFound)
            {
                isDungeonRootFound = parent.TryGetComponent(out dungeonRoot);

                if (isDungeonRootFound)
                    break;
                
                parent = parent.parent;
                isParentFound = parent != null;

                if (iterations > 100)
                {
                    Debug.LogError("Infinity loop!");
                    break;
                }
                
                iterations++;
            }

            if (!isDungeonRootFound)
            {
                Log.PrintError(log: $"<gb>{nameof(DungeonRoot).GetNiceName()}</gb> component <rb>not found</rb>!");
                return;
            }

            SetElevatorFloor(dungeonRoot.Floor);
            _levelProviderObserver.RegisterElevator(this);
        }
    }
}