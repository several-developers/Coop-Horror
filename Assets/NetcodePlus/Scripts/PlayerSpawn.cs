using System.Collections.Generic;
using UnityEngine;

namespace NetcodePlus
{
    public class PlayerSpawn : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField]
        private string _id;

        [SerializeField]
        private int _playerID;

        [SerializeField]
        public float _radius = 1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public int PlayerID => _playerID;

        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<PlayerSpawn> List = new();

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() =>
            List.Add(item: this);

        private void OnDestroy() =>
            List.Remove(item: this);

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetPlayerID(int id) =>
            _playerID = id;

        public Vector3 GetRandomPosition()
        {
            if (_radius <= 0.01f)
                return transform.position;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float rad = Random.Range(0f, _radius);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * rad;
            
            return transform.position + offset;
        }

        public static PlayerSpawn GetNearest(Vector3 pos, float range = 999f)
        {
            PlayerSpawn nearest = null;
            float minDist = range;

            foreach (PlayerSpawn spawn in List)
            {
                float dist = (spawn.transform.position - pos).magnitude;

                if (dist >= minDist)
                    continue;

                minDist = dist;
                nearest = spawn;
            }

            return nearest;
        }

        public static PlayerSpawn GetNearest(Vector3 pos, string id, float range = 999f)
        {
            PlayerSpawn nearest = null;
            float minDist = range;
            
            foreach (PlayerSpawn spawn in List)
            {
                if (spawn._id != id)
                    continue;
                
                float dist = (spawn.transform.position - pos).magnitude;
                    
                if (dist >= minDist)
                    continue;

                minDist = dist;
                nearest = spawn;
            }

            return nearest;
        }

        public static PlayerSpawn Get(int playerID)
        {
            foreach (PlayerSpawn spawn in List)
            {
                if (spawn._playerID == playerID)
                    return spawn;
            }

            return null;
        }

        public static PlayerSpawn Get(string id = "")
        {
            foreach (PlayerSpawn spawn in List)
            {
                if (spawn._id == id)
                    return spawn;
            }

            return null;
        }

        public static List<PlayerSpawn> GetAll() => List;
    }
}