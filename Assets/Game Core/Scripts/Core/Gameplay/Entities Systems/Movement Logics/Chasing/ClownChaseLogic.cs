using System;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Entities;
using GameCore.Gameplay.Entities.Player;
using GameCore.Gameplay.Level;
using UnityEngine;
using UnityEngine.AI;

namespace GameCore.Gameplay.EntitiesSystems.MovementLogics
{
    public class ClownChaseLogic : ChaseLogic
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ClownChaseLogic(IEntity entity, NavMeshAgent agent, ILevelProvider levelProvider) : base(entity, agent)
        {
            _levelProvider = levelProvider;

            ToggleCustomTargetPoint(true);
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Func<EntityLocation> GetClownLocationEvent = () => EntityLocation.Dungeon;
        public event Func<Floor> GetClownFloorEvent = () => Floor.One;
        public event Func<float> GetFireExitInteractionDistanceEvent = () => 0.5f;
        public event Func<float> GetStoppingDistanceEvent = () => 1f;

        private readonly ILevelProvider _levelProvider;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override void Start()
        {
            base.Start();

            GetTargetPositionEvent += GetTargetPosition;
        }

        public override void Stop()
        {
            base.Stop();

            GetTargetPositionEvent -= GetTargetPosition;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckInteractionWithFireExit(FireExit fireExit)
        {
            Transform teleportPoint = fireExit.GetTeleportPoint();
            Vector3 teleportPosition = teleportPoint.position;
            Vector3 clownPosition = Transform.position;
            float distance = Vector3.Distance(a: teleportPosition, b: clownPosition);
            bool canInteract = distance < GetFireExitInteractionDistanceEvent.Invoke();

            if (!canInteract)
                return;

            fireExit.Interact(Entity);
        }

        private Vector3 GetTargetPosition()
        {
            PlayerEntity targetPlayer = GetTargetPlayer();
            EntityLocation playerLocation = targetPlayer.EntityLocation;
            Floor playerFloor = targetPlayer.CurrentFloor;

            EntityLocation clownLocation = GetClownLocationEvent.Invoke();
            Floor clownFloor = GetClownFloorEvent.Invoke();
            Vector3 targetPosition = targetPlayer.transform.position;
            bool useClownPosition = false;
            bool isChasingPlayer = true;

            switch (clownLocation)
            {
                case EntityLocation.LocationSurface:
                    ClownAtSurfaceLogic();
                    break;

                case EntityLocation.Stairs:
                    ClownAtStairsLogic();
                    break;

                case EntityLocation.Dungeon:
                    ClownAtDungeonLogic();
                    break;
            }

            if (useClownPosition)
                targetPosition = Transform.position;

            Agent.stoppingDistance = isChasingPlayer ? GetStoppingDistanceEvent.Invoke() : 0f;

            return targetPosition;

            // LOCAL METHODS: -----------------------------

            void ClownAtSurfaceLogic()
            {
                bool moveToStairsLocation = playerLocation
                    is EntityLocation.Stairs
                    or EntityLocation.Dungeon;

                if (!moveToStairsLocation)
                    return;

                bool isFireExitFound = _levelProvider.TryGetOtherFireExit(Floor.Surface, out FireExit fireExit);

                if (!isFireExitFound)
                    return;

                Transform teleportPoint = fireExit.GetTeleportPoint();
                targetPosition = teleportPoint.position;
                isChasingPlayer = false;

                CheckInteractionWithFireExit(fireExit);
            }

            void ClownAtStairsLogic()
            {
                FireExit fireExit = null;

                bool isFireExitFound = playerLocation switch
                {
                    EntityLocation.LocationSurface => _levelProvider.TryGetStairsFireExit(Floor.Surface, out fireExit),
                    EntityLocation.Dungeon => _levelProvider.TryGetStairsFireExit(playerFloor, out fireExit),
                    _ => false
                };

                if (!isFireExitFound)
                    return;

                Transform teleportPoint = fireExit.GetTeleportPoint();
                targetPosition = teleportPoint.position;
                isChasingPlayer = false;

                CheckInteractionWithFireExit(fireExit);
            }

            void ClownAtDungeonLogic()
            {
                switch (playerLocation)
                {
                    case EntityLocation.LocationSurface:
                        useClownPosition = true;
                        break;

                    case EntityLocation.Stairs:
                        bool isFireExitFound = _levelProvider.TryGetOtherFireExit(clownFloor, out FireExit fireExit);

                        if (!isFireExitFound)
                            break;

                        Transform teleportPoint = fireExit.GetTeleportPoint();
                        targetPosition = teleportPoint.position;
                        isChasingPlayer = false;

                        CheckInteractionWithFireExit(fireExit);
                        break;

                    case EntityLocation.Dungeon:
                        useClownPosition = playerFloor != clownFloor;
                        break;
                }
            }
        }
    }
}