using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Systems.MovementLogics
{
    public class MoveFromStairsToDungeonLogic : MoveToFireExitLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public MoveFromStairsToDungeonLogic(MonoBehaviour coroutineRunner, NavMeshAgent agent,
            ILevelProvider levelProvider) : base(coroutineRunner, agent, levelProvider)
        {
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Func<Floor> GetDungeonFloorEvent = () => Floor.Surface;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override bool TryGetFireExit(out FireExit fireExit)
        {
            Floor dungeonFloor = GetDungeonFloorEvent.Invoke();
            bool isFireExitFound = LevelProvider.TryGetStairsFireExit(dungeonFloor, out fireExit);
            return isFireExitFound;
        }
    }
}