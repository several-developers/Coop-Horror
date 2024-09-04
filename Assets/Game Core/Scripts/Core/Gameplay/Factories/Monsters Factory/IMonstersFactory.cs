using System;
using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Monsters
{
    public interface IMonstersFactory
    {
        UniTask WarmUp();

        UniTask SpawnMonster<TMonsterEntity>(MonsterType monsterType, Vector3 worldPosition, Quaternion rotation,
            Action<string> fail = null, Action<TMonsterEntity> success = null) where TMonsterEntity : MonsterEntityBase;
    }
}