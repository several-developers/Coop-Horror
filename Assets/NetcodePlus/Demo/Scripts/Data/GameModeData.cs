using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus.Demo
{
    public enum GameMode
    {
        None = 0,
        Simple = 10,
        Tank = 20,
        Puzzle = 30,

    }
    [CreateAssetMenu(fileName = "GameMode", menuName = "Data/GameMode", order = 10)]
    public class GameModeData : ScriptableObject
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField]
        private GameMode _mode;
        
        [SerializeField]
        private string _scene;
        
        [SerializeField]
        private int _playersMax = 4;

        // PROPERTIES: ----------------------------------------------------------------------------

        public GameMode Mode => _mode;
        public string Scene => _scene;
        public int PlayersMax => _playersMax;

        // FIELDS: --------------------------------------------------------------------------------
        
        private static readonly List<GameModeData> Modes = new();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public static void Load(string folder = "/")
        {
            Modes.Clear();
            Modes.AddRange(collection: Resources.LoadAll<GameModeData>(folder));
        }

        public static GameModeData GetByScene(string scene)
        {
            foreach (GameModeData choice in Modes)
            {
                if (choice._scene == scene)
                    return choice;
            }
            return null;
        }

        public static GameModeData Get(GameMode mode)
        {
            foreach (GameModeData gameModeData in Modes)
            {
                if (gameModeData._mode == mode)
                    return gameModeData;
            }
            
            return null;
        }

        public static List<GameModeData> GetAll()
        {
            return Modes;
        }
    }
}
