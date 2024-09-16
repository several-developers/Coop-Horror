using Cysharp.Threading.Tasks;

namespace GameCore.Gameplay.Storages.Assets
{
    public interface IWarmUp
    {
        UniTask WarmUp();
    }
}