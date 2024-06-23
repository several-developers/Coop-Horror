using System.Collections;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.EvilClown
{
    public class WanderingTimer
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public WanderingTimer(EvilClownEntity evilClownEntity)
        {
            _evilClownEntity = evilClownEntity;
        }

        // FIELDS: --------------------------------------------------------------------------------

        private const float WanderingTime = 5f;
        
        private readonly EvilClownEntity _evilClownEntity;

        private Coroutine _timerCO;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TryStartTimer()
        {
            if (IsTimerActive())
                return;

            IEnumerator routine = TimerCO();
            _timerCO = _evilClownEntity.StartCoroutine(routine);
        }

        public void StopTimer()
        {
            if (!IsTimerActive())
                return;
            
            _evilClownEntity.StopCoroutine(_timerCO);
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private IEnumerator TimerCO()
        {
            yield return new WaitForSeconds(WanderingTime);
            RunAway();
        }

        private void RunAway() =>
            _evilClownEntity.RunAway();

        private bool IsTimerActive() =>
            _timerCO != null;
    }
}