using UnityEngine;

namespace GameCore.Gameplay.Factories.Player
{
    public interface IPlayerFactory
    {
        void Create(Vector3 at);
    }
}