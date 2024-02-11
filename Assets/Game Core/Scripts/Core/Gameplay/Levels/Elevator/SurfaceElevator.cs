using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels.Elevator
{
    public class SurfaceElevator : ElevatorBase
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Animator _animator;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Start() => OpenElevator();

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OpenElevator() =>
            _animator.SetTrigger(id: AnimatorHashes.Open);
    }
}