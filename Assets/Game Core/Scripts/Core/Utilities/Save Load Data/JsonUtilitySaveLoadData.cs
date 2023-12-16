using System;
using System.IO;
using GameCore.Infrastructure.Data;
using UnityEngine;

namespace GameCore.Utilities
{
    public class JsonUtilitySaveLoadData : ISaveLoadData
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TrySetData<T>(string data, ref T t) where T : DataBase
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            t ??= (T)Activator.CreateInstance(typeof(T));
            t = JsonUtility.FromJson<T>(data);
        }

        public void TryLoadData<T>(ref T t) where T : DataBase
        {
            string path = t.GetDataPath();
            bool isFileExists = File.Exists(path);

            if (!isFileExists)
            {
                t = (T)Activator.CreateInstance(typeof(T));
                string json = JsonUtility.ToJson(t);
                File.WriteAllText(path, json);
            }

            string data = File.ReadAllText(path);
            TrySetData(data, ref t);
        }

        public void TrySaveData<T>(T t) where T : DataBase
        {
            t ??= (T)Activator.CreateInstance(typeof(T));

            string path = t.GetDataPath();
            string data = JsonUtility.ToJson(t);

            File.WriteAllText(path, data);
        }

        public void TryDeleteData<T>(ref T t) where T : DataBase
        {
            t ??= (T)Activator.CreateInstance(typeof(T));

            string path = t.GetDataPath();

            if (File.Exists(path))
                File.Delete(path);

            t = (T)Activator.CreateInstance(typeof(T));
        }
    }
}