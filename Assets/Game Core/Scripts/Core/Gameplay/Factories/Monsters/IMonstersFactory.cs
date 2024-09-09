using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Utilities;

namespace GameCore.Gameplay.Factories.Monsters
{
    public interface IMonstersFactory
    {
        void CreateMonsterDynamic<TMonsterEntity>(MonsterType monsterType, SpawnParams<TMonsterEntity> spawnParams)
            where TMonsterEntity : MonsterEntityBase;
    }
}