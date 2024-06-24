using CustomEditors;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Configs.Gameplay.MobileHeadquarters
{
    public class MobileHeadquartersConfigMeta : EditorMeta
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField, Min(0f)]
        private float _roadLocationMoveSpeed = 40f;
        
        [SerializeField, Min(0f)]
        private float _gameplayLocationMoveSpeed = 10f;
        
        [SerializeField, Min(0f)]
        private float _speedChangeRate = 1f;

        [SerializeField, Range(0f, 1f), SuffixLabel("%", overlay: true)]
        private float _leavingMainRoadSpeedMultiplier = 0.5f;

        [SerializeField, Range(0f, 1f), SuffixLabel("%", overlay: true)]
        private float _startPositionAtRoadLocation;

        // PROPERTIES: ----------------------------------------------------------------------------

        public float MovementSpeed => _roadLocationMoveSpeed;
        public float RoadLocationMoveSpeed => _roadLocationMoveSpeed;
        public float GameplayLocationMoveSpeed => _gameplayLocationMoveSpeed;
        public float SpeedChangeRate => _speedChangeRate;
        public float LeavingMainRoadSpeedMultiplier => _leavingMainRoadSpeedMultiplier;
        public float StartPositionAtRoadLocation => _startPositionAtRoadLocation;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string GetMetaCategory() =>
            EditorConstants.GameplayConfigsCategory;
    }
}