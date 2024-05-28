using System;
using Cinemachine;

namespace GameCore.Gameplay.Level.Locations
{
    public interface ILocationManagerDecorator
    {
        event Func<CinemachinePath> OnGetEnterPathInnerEvent;
        event Func<CinemachinePath> OnGetExitPathInnerEvent;
        CinemachinePath GetEnterPath();
        CinemachinePath GetExitPath();
    }
}