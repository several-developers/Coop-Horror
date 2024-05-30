using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters.Beetle
{
    public abstract class MonsterEntityBase : NetcodeBehaviour, IEntity
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        protected NavMeshAgent _agent;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake() => CheckAgentState();

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public Transform GetTransform() => transform;

        public NavMeshAgent GetAgent() => _agent;

        // PROTECTED METHODS: ---------------------------------------------------------------------
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckAgentState()
        {
            if (!_agent.enabled)
                return;

            Log.PrintError(log: "Nav Mesh Agent enabled! Disable it in prefab!");
        }
    }
}