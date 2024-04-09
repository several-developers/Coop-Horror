using System;
using Cinemachine;

namespace GameCore.Gameplay.Levels.Locations
{
    public class LocationManagerDecorator : ILocationManagerDecorator
    {
        // FIELDS: --------------------------------------------------------------------------------
        
        public event Func<CinemachinePath> OnGetEnterPathEvent;
        public event Func<CinemachinePath> OnGetExitPathEvent;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetEnterPath() =>
            OnGetEnterPathEvent?.Invoke();

        public CinemachinePath GetExitPath() =>
            OnGetExitPathEvent?.Invoke();
    }
}