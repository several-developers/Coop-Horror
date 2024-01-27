using GameCore.Gameplay.InputHandlerTEMP;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Player.Movement
{
    public class PhysicalMovementBehaviour2 : IMovementBehaviour
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public PhysicalMovementBehaviour2(PlayerEntity playerEntity)
        {
            PlayerReferences playerReferences = playerEntity.References;

            _inputReader = playerReferences.InputReader;

            _movementComponent = new MovementComponent(playerEntity, movementBehaviour: this);
            _jumpComponent = new JumpComponent(playerEntity, movementBehaviour: this);
            
            _movementComponent.Init();
            _jumpComponent.Init();

            _inputReader.OnMoveEvent += OnMove;
            _inputReader.OnSprintEvent += OnSprint;
            _inputReader.OnSprintCanceledEvent += OnSprintCanceled;
            _inputReader.OnJumpEvent += OnJump;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public MovementComponent MovementComponent => _movementComponent;
        public JumpComponent JumpComponent => _jumpComponent;

        // FIELDS: --------------------------------------------------------------------------------
        
        private readonly InputReader _inputReader;
        private readonly MovementComponent _movementComponent;
        private readonly JumpComponent _jumpComponent;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Tick()
        {
            _jumpComponent.Tick();
        }

        public void FixedTick()
        {
            _movementComponent.FixedTick();
        }

        public void Dispose()
        {
            _inputReader.OnMoveEvent -= OnMove;
            _inputReader.OnSprintEvent -= OnSprint;
            _inputReader.OnSprintCanceledEvent -= OnSprintCanceled;
            _inputReader.OnJumpEvent -= OnJump;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnMove(Vector2 moveVector) =>
            _movementComponent.SetMoveInput(moveVector);

        private void OnSprint() =>
            _movementComponent.ToggleSprinting(isSprinting: true);

        private void OnSprintCanceled() =>
            _movementComponent.ToggleSprinting(isSprinting: false);

        private void OnJump() =>
            _jumpComponent.PerformJump();
    }
}