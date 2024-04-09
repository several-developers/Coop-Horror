using System;
using Cinemachine;

namespace GameCore.Gameplay.Levels.Locations
{
    public interface ILocationManagerDecorator
    {
        event Func<CinemachinePath> OnGetEnterPathEvent;
        event Func<CinemachinePath> OnGetExitPathEvent;
        CinemachinePath GetEnterPath();
        CinemachinePath GetExitPath();
    }
}