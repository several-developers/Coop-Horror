using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Levels
{
    public class FireExitTeleportPointDrawer : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _drawSphere = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool DrawSphere => _drawSphere;
    }
}