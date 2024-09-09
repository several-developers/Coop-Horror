using Cysharp.Threading.Tasks;

namespace GameCore.Gameplay.AssetsStorages
{
    public interface IWarmUp
    {
        UniTask WarmUp();
    }
}