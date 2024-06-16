using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters.Beetle;
using UnityEngine;

namespace GameCore.Gameplay.Factories.Monsters
{
    public interface IMonstersFactory
    {
        bool SpawnMonster(MonsterType monsterType, Vector3 worldPosition, Quaternion rotation,
            out MonsterEntityBase monsterEntity);
    }
}