using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Level.Locations
{
    public class MetroDoorTeleportPointDrawer : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawSphere = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawSphere => _drawSphere;
    }
}