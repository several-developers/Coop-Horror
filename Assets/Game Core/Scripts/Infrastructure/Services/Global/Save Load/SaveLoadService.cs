using GameCore.Infrastructure.Data;
using GameCore.Infrastructure.Providers.Global.Data;

namespace GameCore.Infrastructure.Services.Global
{
    public class SaveLoadService : ISaveLoadService
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SaveLoadService(IDataProvider dataProvider) =>
            _dataManager = dataProvider.GetDataManager();

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DataManager _dataManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Load() =>
            _dataManager.LoadLocalData();

        public void Save() =>
            _dataManager.SaveLocalData();
    }
}