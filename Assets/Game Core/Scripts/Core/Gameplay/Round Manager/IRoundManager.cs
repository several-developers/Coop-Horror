using GameCore.Enums.Gameplay;

namespace GameCore.Gameplay.RoundManagement
{
    public interface IRoundManager
    {
        float GetLocationDangerValueMultiplier();
        int GetPlayersAmount();
        int GetAlivePlayersAmount();
        int GetCurrentIndoorDangerValue();
        int GetCurrentOutdoorDangerValue();
        int GetMonstersCount(MonsterType monsterType);
    }
}