using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NetcodePlus.Demo
{
    [System.Serializable]
    public class GameData
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        public GameMode mode;
        public PlayerData[] players;

        // FIELDS: --------------------------------------------------------------------------------
        
        private static GameData data = null;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public PlayerData AddNewPlayer(string username)
        {
            for (int i = 0; i < players.Length; i++)
            {
                PlayerData player = players[i];
                
                if (!player.IsAssigned())
                {
                    player.username = username;
                    return player;
                }
            }
            
            return null;
        }

        public PlayerData GetPlayer(int playerID)
        {
            foreach (PlayerData player in players)
            {
                if (player != null && player.player_id == playerID)
                    return player;
            }
            
            return null;
        }

        public PlayerData GetPlayer(string username)
        {
            foreach (PlayerData player in players)
            {
                if (player != null && player.username == username)
                    return player;
            }
            
            return null;
        }

        public PlayerData GetPlayerByCharacter(string character)
        {
            foreach (PlayerData player in players)
            {
                if (player != null && player.character == character)
                    return player;
            }
            
            return null;
        }

        public int CountConnected()
        {
            int count = 0;
            
            foreach (PlayerData player in players)
            {
                if (player != null && player.player_id >= 0 && player.connected)
                    count += 1;
            }
            
            return count;
        }

        public static void Unload() =>
            data = null;

        public static void Override(GameData dat) =>
            data = dat;

        public static GameData Create(GameMode mode, int nbPlayers)
        {
            data = new GameData();
            data.mode = mode;
            data.players = new PlayerData[nbPlayers];
            
            for (int i = 0; i < nbPlayers; i++)
                data.players[i] = new PlayerData(i);
            
            return data;
        }

        public static GameData Get() => data;
    }
}
