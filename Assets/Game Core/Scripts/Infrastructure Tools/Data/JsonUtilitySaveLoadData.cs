using System;
using System.IO;
using UnityEngine;

namespace GameCore.InfrastructureTools.Data
{
    public class JsonUtilitySaveLoadData : ISaveLoadData
    {
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void TrySetData<T>(string data, ref T t) where T : DataBase
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            t ??= (T)Activator.CreateInstance(t.GetType());
            JsonUtility.FromJsonOverwrite(data, t);
        }

        public void TryLoadData<T>(ref T t) where T : DataBase
        {
            Type type = t.GetType();
            string path = GetDataPath(type);
            bool isFileExists = File.Exists(path);

            if (!isFileExists)
            {
                t = (T)Activator.CreateInstance(type);
                string json = JsonUtility.ToJson(t);
                File.WriteAllText(path, json);
            }

            string data = File.ReadAllText(path);
            TrySetData(data, ref t);
        }

        public void TrySaveData<T>(T t) where T : DataBase
        {
            if (t == null)
                return;

            Type type = t.GetType();
            string path = GetDataPath(type);
            string data = JsonUtility.ToJson(t);

            File.WriteAllText(path, data);
        }

        public void TryDeleteData<T>(ref T t) where T : DataBase
        {
            Type type = t.GetType();
            t ??= (T)Activator.CreateInstance(type);

            string path = GetDataPath(type);

            if (File.Exists(path))
                File.Delete(path);
            
            var newT = (T)Activator.CreateInstance(type);
            string data = JsonUtility.ToJson(newT);
            
            TrySetData(data, ref t);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private string GetDataPath(Type type)
        {
            string dataType = type.Name;
            return $"{Application.persistentDataPath}/{dataType}.json";
        }
    }
}