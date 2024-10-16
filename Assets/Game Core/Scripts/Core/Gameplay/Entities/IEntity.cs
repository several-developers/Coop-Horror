﻿using Unity.Netcode;
using UnityEngine;

namespace GameCore.Gameplay.Entities
{
    public interface IEntity
    {
        MonoBehaviour GetMonoBehaviour();
        Transform GetTransform();
        NetworkObject GetNetworkObject();
    }
}