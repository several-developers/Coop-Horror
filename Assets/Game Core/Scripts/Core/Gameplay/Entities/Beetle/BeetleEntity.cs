using GameCore.Configs.Gameplay.Enemies;
using GameCore.Gameplay.Entities.Beetle.States;
using GameCore.Gameplay.Network;
using GameCore.Infrastructure.Providers.Gameplay.MonstersAI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace GameCore.Gameplay.Entities.Beetle
{
    public class BeetleEntity : NetcodeBehaviour, IEntity
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        [Inject]
        private void Construct(IMonstersAIConfigsProvider monstersAIConfigsProvider)
        {
            _beetleAIConfig = monstersAIConfigsProvider.GetBeetleAIConfig();
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private NavMeshAgent _agent;

        // FIELDS: --------------------------------------------------------------------------------

        private BeetleAIConfigMeta _beetleAIConfig;
        private StateMachine _beetleStateMachine;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Transform GetTransform() => transform;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override void InitServerOnly()
        {
            SetupStates();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetupStates()
        {
            IdleState idleState = new();
            
            _beetleStateMachine.AddState(idleState);
            
            _beetleStateMachine.ChangeState<IdleState>();
        }
    }
}