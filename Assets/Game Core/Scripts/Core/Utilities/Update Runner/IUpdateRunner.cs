using System;

namespace GameCore.Utilities
{
    public interface IUpdateRunner
    {
        void Subscribe(Action<float> onUpdate, float updatePeriod);
        void Unsubscribe(Action<float> onUpdate);
    }
}