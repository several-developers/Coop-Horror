using System.Collections.Generic;

namespace GameCore.Gameplay.Storages.Assets
{
    public interface IScenesAssetsStorage : IAssetsStorage<string>
    {
        IEnumerable<string> GetAllScenesPath();
    }
}