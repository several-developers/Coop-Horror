using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class DungeonFloorButton : ControlPanelButton
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private DungeonIndex _dungeonIndex;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public override void Interact()
        {
            ToggleInteract(canInteract: false);
            PlayAnimation();
        }
    }
}