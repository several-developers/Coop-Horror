using UnityEngine.Animations.Rigging;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown.States
{
    public class DespawnState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DespawnState(EvilClownEntity evilClownEntity, Rig rig)
        {
            _evilClownEntity = evilClownEntity;
            _rig = rig;
            _wanderingTimer = evilClownEntity.GetWanderingTimer();
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly EvilClownEntity _evilClownEntity;
        private readonly WanderingTimer _wanderingTimer;
        private readonly Rig _rig;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            StopWanderingTimer();
            DisableRig();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void StopWanderingTimer() =>
            _wanderingTimer.StopTimer();

        private void DisableRig() =>
            _rig.weight = 0f;
    }
}