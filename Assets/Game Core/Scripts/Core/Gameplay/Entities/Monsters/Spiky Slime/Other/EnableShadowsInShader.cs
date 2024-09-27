using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.Gameplay.Entities.Monsters.SpikySlime
{
    public class EnableShadowsInShader : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [Title(Constants.References)]
        [SerializeField, Required]
        private Renderer _renderer;

        // GAME ENGINE METHODS: -------------------------------------------------------------------
        
        private void Awake()
        {
            Material material = _renderer.material;
            material.SetShaderPassEnabled(passName: "ShadowCaster", enabled: true);
        }
    }
}