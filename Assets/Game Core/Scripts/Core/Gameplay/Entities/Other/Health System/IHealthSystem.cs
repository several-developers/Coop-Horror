using System;
using GameCore.Enums;

namespace GameCore.Gameplay.Entities.Other
{
    public interface IHealthSystem
    {
        event Action<HealthStaticData> OnHealthUpdatedEvent;
        event Action OnDeathEvent;
    }
}