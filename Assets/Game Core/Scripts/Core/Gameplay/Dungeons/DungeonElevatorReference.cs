using GameCore.Gameplay.Levels.Elevator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonElevatorReference : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private DungeonElevator _dungeonElevator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => SetReference();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetReference()
        {
            bool isFound = transform.parent.TryGetComponent(out DungeonReferences dungeonReferences);

            if (!isFound)
            {
                string errorLog = Log.HandleLog($"<gb>Dungeon References</gb> component <rb>not found</rb>!");
                Debug.LogError(errorLog);
                return;
            }
            
            dungeonReferences.SetElevator(_dungeonElevator);
        }
    }
}