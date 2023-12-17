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
        private NavMeshSurface _navMeshSurface;

        // PUBLIC METHODS: ------------------------------------------------------------------------
 
        public NavMeshSurface GetNavMeshSurface() => _navMeshSurface;
    }
}