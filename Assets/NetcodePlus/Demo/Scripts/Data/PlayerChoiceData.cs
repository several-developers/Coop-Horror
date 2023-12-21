using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus.Demo
{
    [CreateAssetMenu(fileName = "PlayerChoice", menuName = "Data/PlayerChoice", order = 10)]
    public class PlayerChoiceData : ScriptableObject
    {
        public GameMode mode;
        public string id;
        public GameObject prefab;
        public Color color;

        private static readonly List<PlayerChoiceData> Choices = new();

        public static void Load(string folder = "/")
        {
            Choices.Clear();
            Choices.AddRange(Resources.LoadAll<PlayerChoiceData>(folder));

            TheNetwork network = TheNetwork.Get();

            foreach (PlayerChoiceData choice in Choices)
                network.RegisterPrefab(choice.prefab);
        }

        public static PlayerChoiceData Get(GameMode mode)
        {
            foreach (PlayerChoiceData choice in Choices)
            {
                if (choice.mode == mode)
                    return choice;
            }

            return null;
        }

        public static PlayerChoiceData Get(string id)
        {
            foreach (PlayerChoiceData choice in Choices)
            {
                if (choice.id == id)
                    return choice;
            }

            return null;
        }

        public static PlayerChoiceData Get(GameMode mode, string id)
        {
            foreach (PlayerChoiceData choice in Choices)
            {
                if (choice.mode == mode && choice.id == id)
                    return choice;
            }

            return null;
        }

        public static List<PlayerChoiceData> GetAll(GameMode mode)
        {
            List<PlayerChoiceData> vchoices = new List<PlayerChoiceData>();

            foreach (PlayerChoiceData choice in Choices)
            {
                if (choice.mode == mode)
                    vchoices.Add(choice);
            }

            return vchoices;
        }

        public static List<PlayerChoiceData> GetAll() => Choices;
    }
}