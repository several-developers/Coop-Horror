using System;
using System.Collections;
using GameCore.Infrastructure.Configs.Gameplay.Elevator;
using GameCore.Enums.Gameplay;
using GameCore.Gameplay.Level;
using GameCore.Gameplay.Level.Elevator;
using GameCore.Gameplay.Network;
using GameCore.Gameplay.Systems.Utilities;
using GameCore.Gameplay.Managers.Visual;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Level.Elevator
{
    public class ElevatorMovementSystem
    {
        public enum MovementState
        {
            Idle = 0,
            MoveUpToTeleportPoint = 1,
            MoveDownToTeleportPoint = 2,
            MoveToStartPoint = 3
        }

        // CONSTRUCTORS: --------------------------------------------------------------------------

        public ElevatorMovementSystem(
            ElevatorEntity elevatorEntity,
            ILevelProvider levelProvider,
            IVisualManager visualManager
        )
        {
            _elevatorEntity = elevatorEntity;
            _elevatorConfig = elevatorEntity.GetElevatorConfig();
            _references = elevatorEntity.GetReferences();
            _levelProvider = levelProvider;
            _visualManager = visualManager;
            _transform = elevatorEntity.transform;
            _movementLogicRoutine = new CoroutineHelper(elevatorEntity);
            _lastTeleportPoint = _transform.position;

            _movementLogicRoutine.GetRoutineEvent += MovementLogicCO;
        }

        // FIELDS: --------------------------------------------------------------------------------

        public event Action OnElevatorStoppedEvent = delegate { };
        public event Action OnElevatorFloorChangedEvent = delegate { };

        private readonly ElevatorEntity _elevatorEntity;
        private readonly ElevatorConfigMeta _elevatorConfig;
        private readonly ElevatorReferences _references;
        private readonly ILevelProvider _levelProvider;
        private readonly IVisualManager _visualManager;
        private readonly Transform _transform;
        private readonly CoroutineHelper _movementLogicRoutine;

        private Vector3 _lastTeleportPoint;
        private MovementState _movementState;
        private bool _reachedTargetFloor;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void StartMovement()
        {
            CheckMovementState();

            if (_movementState == MovementState.Idle)
                return;

            _movementLogicRoutine.Start();
        }

        public ElevatorEntity.ElevatorState GetElevatorState()
        {
            return _movementState switch
            {
                MovementState.MoveUpToTeleportPoint => ElevatorEntity.ElevatorState.MovingUp,
                MovementState.MoveDownToTeleportPoint => ElevatorEntity.ElevatorState.MovingDown,
                _ => ElevatorEntity.ElevatorState.Idle
            };
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckMovementState()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();

            if (currentFloor == targetFloor)
            {
                _movementState = MovementState.Idle;
                return;
            }

            int targetFloorIndex = (int)targetFloor;
            int currentFloorIndex = (int)currentFloor;

            _movementState = targetFloorIndex > currentFloorIndex
                ? MovementState.MoveDownToTeleportPoint
                : MovementState.MoveUpToTeleportPoint;
        }

        private void CheckIfReachedTargetFloor()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();
            _reachedTargetFloor = currentFloor == targetFloor;
        }

        private void ChangeFloor()
        {
            Floor currentFloor = GetCurrentFloor();
            Floor targetFloor = GetTargetFloor();

            if (currentFloor == targetFloor)
                return;

            int targetFloorIndex = (int)targetFloor;
            int currentFloorIndex = (int)currentFloor;

            int newFloorIndex = targetFloorIndex > currentFloorIndex
                ? currentFloorIndex + 1
                : currentFloorIndex - 1;

            var newFloor = (Floor)newFloorIndex;
            _elevatorEntity.SetCurrentFloor(newFloor);
            
            OnElevatorFloorChangedEvent.Invoke();

            switch (newFloor)
            {
                case Floor.Surface:
                    _visualManager.SetLocationPreset();
                    break;
                
                default:
                    _visualManager.ChangePreset(VisualPresetType.Dungeon);
                    break;
            }
        }

        private void TeleportToSection()
        {
            Floor currentFloor = GetCurrentFloor();

            bool isElevatorMovePointFound =
                _levelProvider.TryGetElevatorMovePoint(currentFloor, out ElevatorMovePoint elevatorMovePoint);

            if (!isElevatorMovePointFound)
                return;

            Transform elevatorPoint = elevatorMovePoint.transform;
            _lastTeleportPoint = elevatorPoint.position;

            Vector3 teleportPosition = _lastTeleportPoint;
            float movementOffsetY = _elevatorConfig.MovementOffsetY;

            switch (_movementState)
            {
                case MovementState.MoveUpToTeleportPoint:
                    teleportPosition.y -= movementOffsetY;
                    break;

                case MovementState.MoveDownToTeleportPoint:
                    teleportPosition.y += movementOffsetY;
                    break;
            }

            ClientNetworkTransform networkTransform = _references.NetworkTransform;
            networkTransform.Teleport(teleportPosition, elevatorPoint.rotation, newScale: Vector3.one);
        }

        private IEnumerator MovementLogicCO()
        {
            yield return MovementStartDelayCO();
            yield return MoveToTeleportPointCO();
            yield return ChangeFloorCO();

            TeleportToSection();

            _movementState = MovementState.MoveToStartPoint;

            yield return MoveToTeleportPointCO();
            _elevatorEntity.PlaySound(ElevatorEntity.SFXType.DoorOpening).Forget();
            yield return DoorOpenDelay();

            OnElevatorStoppedEvent?.Invoke();

            // LOCAL METHODS: -----------------------------

            IEnumerator MovementStartDelayCO()
            {
                float movementDelay = _elevatorConfig.MovementDelay;
                yield return new WaitForSeconds(movementDelay);
            }

            IEnumerator MoveToTeleportPointCO()
            {
                float startPositionY = _transform.position.y;
                float endPositionY = GetEndPositionY();
                float movementDuration = GetMovementDuration();
                movementDuration *= 0.5f; // 50% времени убытие, 50% времени прибытие 

                float elapsedTime = 0f;

                while (elapsedTime < movementDuration)
                {
                    elapsedTime += Time.deltaTime;

                    float time = elapsedTime / movementDuration;
                    float interpolatedValue = GetInterpolatedValue(startPositionY, endPositionY, time);

                    _transform.position =
                        new Vector3(x: _lastTeleportPoint.x, y: interpolatedValue, z: _lastTeleportPoint.z);

                    yield return null;
                }

                _transform.position = new Vector3(x: _lastTeleportPoint.x, y: endPositionY, z: _lastTeleportPoint.z);
            }

            IEnumerator ChangeFloorCO()
            {
                float movementDuration = GetMovementDuration();

                do
                {
                    ChangeFloor();
                    CheckIfReachedTargetFloor();

                    if (_reachedTargetFloor)
                        break;

                    yield return new WaitForSeconds(movementDuration);
                } while (!_reachedTargetFloor);
            }

            IEnumerator DoorOpenDelay()
            {
                float doorOpenDelay = _elevatorConfig.DoorOpenDelay;
                yield return new WaitForSeconds(doorOpenDelay);
            }
        }

        private AnimationCurve GetAnimationCurve()
        {
            return _movementState switch
            {
                MovementState.MoveToStartPoint => _elevatorConfig.SlowDownCurve,
                _ => _elevatorConfig.SpeedUpCurve
            };
        }

        private Floor GetCurrentFloor() =>
            _elevatorEntity.GetCurrentElevatorFloor();

        private Floor GetTargetFloor() =>
            _elevatorEntity.GetTargetElevatorFloor();

        private float GetMovementDuration() =>
            _elevatorConfig.MovementDurationPerFloor;

        private float GetInterpolatedValue(float startPositionY, float endPositionY, float time)
        {
            AnimationCurve animationCurve = GetAnimationCurve();
            return Mathf.Lerp(a: startPositionY, b: endPositionY, t: animationCurve.Evaluate(time));
        }

        private float GetEndPositionY()
        {
            float movementOffsetY = _elevatorConfig.MovementOffsetY;

            switch (_movementState)
            {
                case MovementState.Idle:
                case MovementState.MoveToStartPoint:
                default:
                    return _lastTeleportPoint.y;

                case MovementState.MoveUpToTeleportPoint:
                    return _lastTeleportPoint.y + movementOffsetY;

                case MovementState.MoveDownToTeleportPoint:
                    return _lastTeleportPoint.y - movementOffsetY;
            }
        }
    }
}