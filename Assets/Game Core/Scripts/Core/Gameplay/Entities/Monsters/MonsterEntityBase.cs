using System;
using GameCore.Enums.Gameplay;
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
        private ClientNetworkTransform _networkTransform;
        
        [SerializeField, Required]
        protected NavMeshAgent _agent;

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnEntityTeleportedEvent = delegate { };

        private EntityLocation _entityLocation;
        
        // GAME ENGINE METHODS: -------------------------------------------------------------------

        protected virtual void Awake() => CheckAgentState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public virtual void Teleport(Vector3 position, Quaternion rotation)
        {
            _agent.enabled = false;
            _networkTransform.Teleport(position, rotation, transform.localScale);
            _agent.enabled = true;
            
            OnEntityTeleportedEvent.Invoke();
        }
        
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