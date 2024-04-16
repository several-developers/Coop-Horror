using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.GameTimeManagement
{
    public class Sun : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Light _light;

        // PROPERTIES: ----------------------------------------------------------------------------

        public Light Light => _light;
    }
}