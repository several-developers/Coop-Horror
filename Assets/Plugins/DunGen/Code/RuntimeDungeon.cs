using UnityEngine;

namespace DunGen
{
    [AddComponentMenu("DunGen/Runtime Dungeon")]
    public class RuntimeDungeon : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        public DungeonGenerator Generator = new();
        public bool GenerateOnStart = true;
        public GameObject Root;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        protected virtual void Start()
        {
            if (GenerateOnStart)
                Generate();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void Generate()
        {
            if (Root != null)
                Generator.Root = Root;

            if (!Generator.IsGenerating)
                Generator.Generate();
        }

        public void GenerateWithSeed(int seed)
        {
            if (Root != null)
                Generator.Root = Root;

            if (Generator.IsGenerating)
                return;

            Generator.ShouldRandomizeSeed = false;
            Generator.Seed = seed;
            
            Generator.Generate();
        }

        public void Clear()
        {
            if (!Generator.IsGenerating)
                Generator.Clear(stopCoroutines: true);
        }
    }
}