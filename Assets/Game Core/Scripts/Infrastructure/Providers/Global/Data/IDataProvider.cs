﻿using GameCore.InfrastructureTools.Data;

namespace GameCore.Infrastructure.Providers.Global.Data
{
    public interface IDataProvider
    {
        DataManager GetDataManager();
        T GetData<T>() where T : DataBase;
    }
}