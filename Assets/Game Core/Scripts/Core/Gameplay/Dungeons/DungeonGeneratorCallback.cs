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
        
        // FIELDS: --------------------------------------------------------------------------------

        public event Action<DungeonIndex> OnGenerationCompletedEvent;

        // EVENTS RECEIVERS: ----------------------------------------------------------------------
        
        public void OnDungeonComplete(Dungeon dungeon) =>
            OnGenerationCompletedEvent?.Invoke(_dungeonIndex);
    }
}