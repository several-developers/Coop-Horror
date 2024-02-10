using System;
using DunGen;
using GameCore.Enums.Gameplay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonGeneratorCallback : MonoBehaviour, IDungeonCompleteReceiver
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private DungeonIndex _dungeonIndex;

        [Title(Constants.References)]
        [SerializeField, Required]
        private DungeonReferences _dungeonReferences;
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DungeonIndex, DungeonReferences> OnGenerationCompletedEvent;
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void DungeonGenerationCompleteLogic(Dungeon dungeon)
        {
            _dungeonReferences.SetDungeon(dungeon);
            
            OnGenerationCompletedEvent?.Invoke(_dungeonIndex, _dungeonReferences);
        }
        
        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        public void OnDungeonComplete(Dungeon dungeon) => DungeonGenerationCompleteLogic(dungeon);
    }
}