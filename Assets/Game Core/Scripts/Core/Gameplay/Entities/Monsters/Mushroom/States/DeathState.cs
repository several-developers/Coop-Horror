﻿using GameCore.Gameplay.Systems.Ragdoll;

namespace GameCore.Gameplay.Entities.Monsters.Mushroom.States
{
    public class DeathState : IEnterState
    {
        // CONSTRUCTORS: --------------------------------------------------------------------------

        public DeathState(MushroomEntity mushroomEntity)
        {
            _mushroomEntity = mushroomEntity;
            _references = mushroomEntity.GetReferences();
        }

        // FIELDS: --------------------------------------------------------------------------------

        private readonly MushroomEntity _mushroomEntity;
        private readonly MushroomReferences _references;

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Enter()
        {
            SetDeadEmotion();
            DisableAgent();
            DisableSuspicionSystem();
            DisableWhisperingSystem();
            EnableRagdoll();
            DisableHidingState();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void SetDeadEmotion() =>
            _mushroomEntity.SetEmotion(MushroomEntity.Emotion.Dead);

        private void DisableAgent() =>
            _mushroomEntity.DisableAgent();

        private void DisableSuspicionSystem()
        {
            SuspicionSystem suspicionSystem = _mushroomEntity.GetSuspicionSystem();
            suspicionSystem.Stop();
        }

        private void DisableWhisperingSystem()
        {
            WhisperingSystem whisperingSystem = _mushroomEntity.GetWhisperingSystem();
            whisperingSystem.Stop();
        }

        private void EnableRagdoll()
        {
            RagdollController ragdollController = _references.RagdollController;
            ragdollController.EnableRagdoll();
        }

        private void DisableHidingState() =>
            _mushroomEntity.SetHidingState(isHiding: false, instant: true);
    }
}