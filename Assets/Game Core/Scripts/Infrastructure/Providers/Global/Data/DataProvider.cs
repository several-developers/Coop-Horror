using System;
using GameCore.InfrastructureTools.Data;

namespace GameCore.Infrastructure.Providers.Global.Data
{
    public class DataProvider : IDataProvider, IDisposable
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DataProvider()
        {
            _dataManager = new DataManager();
            _dataManager.LoadLocalData();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly DataManager _dataManager;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Dispose() =>
            _dataManager.SaveLocalData();

        public DataManager GetDataManager() => _dataManager;
        
        public T GetData<T>() where T : DataBase =>
            _dataManager.GetData<T>();
    }
}