using System;
using GameCore.Enums;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Other
{
    public class ZombieCrawlingHandler
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ZombieCrawlingHandler(IHealthSystem healthSystem, Animator animator)
        {
            _healthSystem = healthSystem;
            _animator = animator;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnCrawlingEnabledEvent;

        private readonly IHealthSystem _healthSystem;
        private readonly Animator _animator;
        
        private bool _isCrawlingEnabled;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void EnableCrawling()
        {
            if (_isCrawlingEnabled)
                return;

            _isCrawlingEnabled = true;
            _animator.SetBool(id: AnimatorHashes.IsCrawling, value: true);
            OnCrawlingEnabledEvent?.Invoke();
        }
    }
}