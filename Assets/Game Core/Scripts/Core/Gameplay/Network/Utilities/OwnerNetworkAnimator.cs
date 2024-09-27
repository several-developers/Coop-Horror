using Unity.Netcode.Components;

namespace GameCore.Gameplay.Network
{
    public class OwnerNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}