using Cysharp.Threading.Tasks;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Monsters;
using GameCore.Gameplay.Utilities;

namespace GameCore.Gameplay.Factories.Monsters
{
    public interface IMonstersFactory
    {
        UniTask WarmUp();

        void CreateMonster<TMonsterEntity>(MonsterType monsterType, EntitySpawnParams<TMonsterEntity> spawnParams)
            where TMonsterEntity : MonsterEntityBase;
    }
}