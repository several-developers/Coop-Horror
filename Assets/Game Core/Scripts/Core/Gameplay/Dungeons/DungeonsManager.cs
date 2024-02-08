using DunGen;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Network;
using GameCore.Observers.Gameplay.Dungeons;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Dungeons
{
    public class DungeonsManager : MonoBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IDungeonsObserver dungeonsObserver) =>
            _dungeonsObserver = dungeonsObserver;

        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private RuntimeDungeon[] _dungeons;

        [SerializeField, Required]
        private DungeonGeneratorCallback[] _dungeonCallbacks;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static DungeonsManager _instance;

        private IDungeonsObserver _dungeonsObserver;
        private int _generatedDungeonsAmount;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _instance = this;

            foreach (DungeonGeneratorCallback dungeonCallback in _dungeonCallbacks)
                dungeonCallback.OnGenerationCompletedEvent += OnGenerationCompleted;
        }

        private void Start()
        {
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.OnGenerateDungeonsEvent += OnGenerateDungeons;
        }

        private void OnDestroy()
        {
            RpcCaller rpcCaller = RpcCaller.Get();
            rpcCaller.OnGenerateDungeonsEvent -= OnGenerateDungeons;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void ClearDungeons()
        {
            _generatedDungeonsAmount = 0;
            
            foreach (RuntimeDungeon dungeon in _dungeons)
                dungeon.Clear();
        }
        
        public static DungeonsManager Get() => _instance;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void GenerateDungeons()
        {
            _generatedDungeonsAmount = 0;
            
            foreach (RuntimeDungeon dungeon in _dungeons)
                dungeon.Generate();
        }
        
        private void GenerateDungeonsWithSeed(DungeonsSeedData data)
        {
            _generatedDungeonsAmount = 0;
            
            int iterations = _dungeons.Length;

            for (int i = 0; i < iterations; i++)
            {
                int seed = i switch
                {
                    0 => data.SeedOne,
                    1 => data.SeedTwo,
                    2 => data.SeedThree,
                    _ => 0
                };

                _dungeons[i].GenerateWithSeed(seed);
            }
        }

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnGenerationCompleted(DungeonIndex dungeonIndex)
        {
            _generatedDungeonsAmount++;
            
            if (_generatedDungeonsAmount >= 3)
                _dungeonsObserver.SendDungeonsGenerationCompleted();
        }

        private void OnGenerateDungeons(DungeonsSeedData data) => GenerateDungeonsWithSeed(data);

        // DEBUG BUTTONS: -------------------------------------------------------------------------

        [Title(Constants.DebugButtons)]
        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugGenerateDungeons() => GenerateDungeons();
        
        [Button(buttonSize: 30, ButtonStyle.FoldoutButton), DisableInEditorMode]
        private void DebugGenerateDungeonsWithSeed(int seedOne, int seedTwo, int seedThree)
        {
            DungeonsSeedData data = new(seedOne, seedTwo, seedThree);
            GenerateDungeonsWithSeed(data);
        }

        [Button(buttonSize: 30), DisableInEditorMode]
        private void DebugClearDungeons() => ClearDungeons();
    }
}