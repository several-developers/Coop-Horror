using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.Managers.Round
{
    public interface IRoundManager
    {
        float GetLocationDangerValueMultiplier();
        int GetPlayersAmount();
        int GetAlivePlayersAmount();
        int GetCurrentIndoorDangerValue();
        int GetCurrentOutdoorDangerValue();
        int GetMonstersCount(MonsterType monsterType);
        bool IsAtLeastOneMonsterExists(MonsterType monsterType);
    }
}