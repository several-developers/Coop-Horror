using System.Collections.Generic;
using GameCore.Configs.Gameplay.Enemies;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.NoiseManagement;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using GameCore.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace GameCore.Gameplay.Entities.Monsters.BlindCreature
{
    public class BlindCreatureEntity : MonsterEntityBase, INoiseListener
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMonstersAIConfigsProvider monstersAIConfigsProvider) =>
            _blindCreatureAIConfig = monstersAIConfigsProvider.GetBlindCreatureAIConfig();

        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField, Required, Space(height: 5)]
        private TextMeshPro _stateTMP;
        
        // FIELDS: --------------------------------------------------------------------------------

        private static readonly List<BlindCreatureEntity> AllBlindCreatures = new();
        
        private BlindCreatureAIConfigMeta _blindCreatureAIConfig;
        
        private StateMachine _blindCreatureStateMachine;
        private PlayerEntity _targetPlayer;

        private bool _isDead;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start()
        {
            if (!IsServerOnly)
                return;
            
            // TEMP
            if (!IsSpawned)
                NetworkObject.Spawn();
        }
        
        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void SetTargetPlayer(PlayerEntity playerEntity) =>
            _targetPlayer = playerEntity;
        
        public void DetectNoise(Vector3 noisePosition, float noiseLoudness)
        {
            Debug.Log("Noise detected!");
        }

        public static IReadOnlyList<BlindCreatureEntity> GetAllBlindCreatures() => AllBlindCreatures;

        public override MonsterType GetMonsterType() =>
            MonsterType.BlindCreature;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            AllBlindCreatures.Add(item: this);
            
            InitSystems();
            SetupStates();

            _blindCreatureStateMachine.OnStateChangedEvent += state =>
            {
                string stateName = state.GetType().Name.Replace(oldValue: "State", newValue: "").GetNiceName();
                _stateTMP.text = $"State: {stateName}";
            };
            
            // LOCAL METHODS: -----------------------------

            void InitSystems()
            {
                _blindCreatureStateMachine = new StateMachine();
            }

            void SetupStates()
            {
                
            }
        }

        protected override void TickServerOnly() =>
            _blindCreatureStateMachine.Tick();

        protected override void DespawnServerOnly() =>
            AllBlindCreatures.Remove(item: this);

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeState<TState>() where TState : IState =>
            _blindCreatureStateMachine.ChangeState<TState>();
    }
}