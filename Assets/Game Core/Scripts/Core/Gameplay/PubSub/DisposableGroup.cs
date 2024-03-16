using System;
using System.Collections.Generic;

namespace GameCore.Gameplay.PubSub
{
    public class DisposableGroup : IDisposable
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly List<IDisposable> _disposables = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();

            _disposables.Clear();
        }

        public void Add(IDisposable disposable) =>
            _disposables.Add(disposable);
    }
}