using GameCore.Enums.Gameplay;
using UnityEngine.AddressableAssets;

namespace GameCore.Gameplay.AssetsStorages
{
    public interface IMonstersAssetsStorage : IWarmUp
    {
        bool TryGetAssetReference(MonsterType monsterType, out AssetReferenceGameObject assetReference);
    }
}