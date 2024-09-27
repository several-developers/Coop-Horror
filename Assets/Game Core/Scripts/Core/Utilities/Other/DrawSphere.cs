using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Utilities
{
    public class DrawSphere : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.Settings)]
        [SerializeField]
        private bool _draw;
        
        [SerializeField, Min(0f)]
        private float _radius = 1f;

        [SerializeField]
        private Color _color = Color.green;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_draw)
                return;
        
            Vector3 position = transform.position;
            Color gizmosColor = Gizmos.color;

            Gizmos.color = _color;
            Gizmos.DrawWireSphere(position, _radius);

            Gizmos.color = gizmosColor;
        }
#endif

    }
}