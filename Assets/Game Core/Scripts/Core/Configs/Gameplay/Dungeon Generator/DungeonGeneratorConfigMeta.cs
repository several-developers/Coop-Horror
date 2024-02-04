using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.DungeonGenerator
{
    public class DungeonGeneratorConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private float _dungeonSpawnOffsetY = -100f;
        
        [SerializeField, MinMaxSlider(minValue: 0, maxValue: 25, showFields: true)]
        private Vector2Int _roomsAmount;
        
        [SerializeField, Range(0, 100), SuffixLabel("%", overlay: true)]
        private int _hallwaySpawnChance = 35;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float DungeonSpawnOffsetY => _dungeonSpawnOffsetY;
        public Vector2Int RoomsAmount => _roomsAmount;
        public int HallwaySpawnChance => _hallwaySpawnChance;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}