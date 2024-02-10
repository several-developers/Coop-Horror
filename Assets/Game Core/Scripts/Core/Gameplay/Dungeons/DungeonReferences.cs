using DunGen;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonReferences : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, ReadOnly]
        private Dungeon _dungeon;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetDungeon(Dungeon dungeon) =>
            _dungeon = dungeon;
    }
}