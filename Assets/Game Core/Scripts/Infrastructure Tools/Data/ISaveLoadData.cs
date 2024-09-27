namespace GameCore.InfrastructureTools.Data
{
    public interface ISaveLoadData
    {
        void TrySetData<T>(string data, ref T t) where T : DataBase;
        void TryLoadData<T>(ref T t) where T : DataBase;
        void TrySaveData<T>(T t) where T : DataBase;
        void TryDeleteData<T>(ref T t) where T : DataBase;
    }
}