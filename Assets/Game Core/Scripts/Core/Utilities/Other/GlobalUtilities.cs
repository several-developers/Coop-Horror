using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Utilities
{
    public static class GlobalUtilities
    {
        // Formatting number: 2530 -> 2.530
        public static string FormatNumber(float number) =>
            number.ToString("N0", CultureInfo.GetCultureInfo("de"));

        public static string FormatNumber(double number) =>
            number.ToString("N0", CultureInfo.GetCultureInfo("de"));

        public static string ReplaceCommaWithDot(this string text) =>
            text.Replace(',', '.');

        public static string GetUniqueID() =>
            Guid.NewGuid().ToString().Remove(0, 20);

        /// <summary>
        /// Rule: RandomChance() < your_chance
        /// </summary>
        public static int RandomChance() =>
            Random.Range(0, 99);
        
        public static int GetRandomIndex(params double[] chances)
        {
            System.Random random = new();
            double totalChance = 0;

            foreach (double chance in chances)
                totalChance += chance;

            // Генерируем случайное число в диапазоне от 0 до суммарного шанса
            double randomNumber = random.NextDouble() * totalChance;

            double cumulativeChance = 0;

            // Проверяем, в какой диапазон попало случайное число
            for (int i = 0; i < chances.Length; i++)
            {
                cumulativeChance += chances[i];

                if (randomNumber < cumulativeChance)
                {
                    // Возвращаем предмет с соответствующим индексом
                    return i;
                }
            }

            // Если случайное число не попало ни в один диапазон, возвращаем null
            return 0;
        }

        /// <summary>
        /// Returns true if chance successful.
        /// </summary>
        /// <param name="chance">Your chance (0 - 100)</param>
        /// <returns></returns>
        public static bool IsRandomSuccessful(int chance)
        {
            chance = Mathf.Clamp(chance, 0, 100);
            return Random.Range(0, 99) < chance;
        }

        public static IEnumerable<T> FindAllMeta<T>() where T : ScriptableObject
        {
            List<T> list = new();

#if UNITY_EDITOR
            Type type = typeof(T);
            var guids = AssetDatabase.FindAssets("t:" + type.Name);

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (obj != null && !list.Contains(obj))
                    list.Add(obj);
            }
#endif

            return list;
        }
    }
}