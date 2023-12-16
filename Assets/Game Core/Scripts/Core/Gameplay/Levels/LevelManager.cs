using Cinemachine;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class LevelManager : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private CinemachinePath _playerPath;

        [SerializeField, Required]
        private NavMeshSurface _navMeshSurface;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public CinemachinePath GetCinemachinePath() => _playerPath;

        public NavMeshSurface GetNavMeshSurface() => _navMeshSurface;
    }
}