using UnityEngine;

namespace GameCore.Gameplay.Network.PrefabsRegistrar
{
    public interface INetworkPrefabsRegistrar
    {
        void Register(GameObject prefab);
        void Remove(GameObject prefab);
    }
}