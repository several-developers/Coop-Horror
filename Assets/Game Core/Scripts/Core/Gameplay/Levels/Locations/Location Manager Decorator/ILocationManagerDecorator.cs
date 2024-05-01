using System;
using Cinemachine;

namespace GameCore.Gameplay.Levels.Locations
{
    public interface ILocationManagerDecorator
    {
        event Func<CinemachinePath> OnGetEnterPathInnerEvent;
        event Func<CinemachinePath> OnGetExitPathInnerEvent;
        CinemachinePath GetEnterPath();
        CinemachinePath GetExitPath();
    }
}