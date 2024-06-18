using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.MovementLogics
{
    public class MoveFromStairsToSurfaceLogic : MoveToFireExitLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------
        
        public MoveFromStairsToSurfaceLogic(MonoBehaviour coroutineRunner, NavMeshAgent agent,
            ILevelProvider levelProvider) : base(coroutineRunner, agent, levelProvider)
        {
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        protected override bool TryGetFireExit(out FireExit fireExit) =>
            LevelProvider.TryGetStairsFireExit(Floor.Surface, out fireExit);
    }
}