using System.Collections.Generic;
using GameCore.Gameplay.Entities.Player;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class SpikesAttachSystem
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public SpikesAttachSystem(SpikySlimeEntity spikySlimeEntity) =>
            _references = spikySlimeEntity.GetReferences();

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly SpikySlimeReferences _references;
        
        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public void AttachPlayerToFreeJoint(ulong clientID)
        {
            bool isPlayerFound = PlayerEntity.TryGetPlayer(clientID, out PlayerEntity playerEntity);

            if (!isPlayerFound)
                return;
            
            IReadOnlyList<SpringJoint> allSpringJoints = _references.GetAllSpringJoints();

            FindClosestFreeSprintJoint();

            // LOCAL METHODS: -----------------------------

            void FindFreeSprintJoint()
            {
                foreach (SpringJoint springJoint in allSpringJoints)
                {
                    bool isFree = springJoint.connectedBody == null;

                    if (!isFree)
                        continue;

                    PlayerReferences playerReferences = playerEntity.GetReferences();
                    Rigidbody spineRigidbody = playerReferences.SpineRigidbody;
                    springJoint.connectedBody = spineRigidbody;

                    break;
                }
            }

            void FindClosestFreeSprintJoint()
            {
                Vector3 playerPosition = playerEntity.transform.position;
                int iterations = allSpringJoints.Count;
                float minDistance = float.MaxValue;
                int closestIndex = 0;
                bool isFreeJointFound = false;

                for (int i = 0; i < iterations; i++)
                {
                    SpringJoint springJoint = allSpringJoints[i];
                    bool isFree = springJoint.connectedBody == null;

                    if (!isFree)
                        continue;

                    isFreeJointFound = true;

                    Vector3 jointPosition = springJoint.transform.position;
                    float distance = Vector3.Distance(a: playerPosition, b: jointPosition);

                    if (distance >= minDistance)
                        continue;

                    minDistance = distance;
                    closestIndex = i;
                }

                if (!isFreeJointFound)
                    return;

                PlayerReferences playerReferences = playerEntity.GetReferences();
                Rigidbody spineRigidbody = playerReferences.SpineRigidbody;
                allSpringJoints[closestIndex].connectedBody = spineRigidbody;
            }
        }
        
        public void FreeAllFromSpringJoints()
        {
            IReadOnlyList<SpringJoint> allSpringJoints = _references.GetAllSpringJoints();

            foreach (SpringJoint springJoint in allSpringJoints)
                springJoint.connectedBody = null;
        }
    }
}