using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Player
{
    public interface IPlayerFactory
    {
        PlayerEntity Create(Vector3 at);
    }
}