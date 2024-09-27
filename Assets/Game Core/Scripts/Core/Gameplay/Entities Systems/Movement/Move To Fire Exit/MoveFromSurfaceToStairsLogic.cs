using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Systems.Movement
{
    public class MoveFromSurfaceToStairsLogic : MoveToFireExitLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public MoveFromSurfaceToStairsLogic(MonoBehaviour coroutineRunner, NavMeshAgent agent,
            ILevelProvider levelProvider) : base(coroutineRunner, agent, levelProvider) { }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override bool TryGetFireExit(out FireExit fireExit) =>
            LevelProvider.TryGetOtherFireExit(Floor.Surface, out fireExit);
    }
}