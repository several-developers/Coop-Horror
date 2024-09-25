using GameCore.Gameplay.Network;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.Entities.Monsters
{
    public abstract class NavmeshMonsterEntityBase : MonsterEntityBase
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [SerializeField, Required]
        protected NavMeshAgent _agent;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake() => CheckAgentState();

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Teleport(Vector3 position, Quaternion rotation, bool resetVelocity = false)
        {
            if (resetVelocity && TryGetComponent(out Rigidbody rigidBody))
                rigidBody.velocity = Vector3.zero;
            
            _agent.enabled = false;
            NetworkTransform.Teleport(position, rotation, transform.localScale);
            _agent.enabled = true;

            SendEntityTeleportedEvent();
        }

        public void EnableAgent() =>
            _agent.enabled = true;
        
        public void DisableAgent() =>
            _agent.enabled = false;
        
        public NavMeshAgent GetAgent() => _agent;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckAgentState()
        {
            if (!NetworkHorror.IsTrueServer)
                return;

            if (!_agent.enabled)
            {
                _agent.enabled = true;
                return;
            }

            Log.PrintError(log: "Nav Mesh Agent enabled! Disable it in prefab!");
        }
    }
}