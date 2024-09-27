using System;
using System.Collections;
using UnityEngine;

namespace GameCore.Gameplay.Systems.Utilities
{
    public class CoroutineHelper
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public CoroutineHelper(MonoBehaviour coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        // FIELDS: --------------------------------------------------------------------------------

        public event Func<IEnumerator> GetRoutineEvent = () => null;

        private readonly MonoBehaviour _coroutineRunner;

        private Coroutine _coroutine;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Start()
        {
            Stop();
            
            IEnumerator routine = GetRoutineEvent.Invoke();
            bool isValid = routine != null;

            if (!isValid)
                return;

            _coroutine = _coroutineRunner.StartCoroutine(routine);
        }

        public void Stop()
        {
            if (_coroutine == null)
                return;
            
            _coroutineRunner.StopCoroutine(_coroutine);
        }
    }
}