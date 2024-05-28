using System;
using Cinemachine;

namespace GameCore.Gameplay.Level.Locations
{
    public class LocationManagerDecorator : ILocationManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Func<CinemachinePath> OnGetEnterPathInnerEvent;
        public event Func<CinemachinePath> OnGetExitPathInnerEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetEnterPath() =>
            OnGetEnterPathInnerEvent?.Invoke();

        public CinemachinePath GetExitPath() =>
            OnGetExitPathInnerEvent?.Invoke();
    }
}