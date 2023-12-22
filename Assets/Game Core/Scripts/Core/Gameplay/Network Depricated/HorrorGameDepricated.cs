using NetcodePlus;
using NetcodePlus.Demo;

namespace GameCore.Gameplay.NetworkDepricated
{
    public class HorrorGameDepricated : SMonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            StartGame();
        }

        private void StartGame()
        {
            TheNetwork network = TheNetwork.Get();

            if (network.IsActive())
                return;

            //Start in test mode, when running directly from Unity Scene
            Authenticator.Get().LoginTest("Player"); //May not work with more advanced auth system, works in Test mode
            DemoConnectData cdata = new(GameMode.Simple);
            network.SetConnectionExtraData(cdata);
            network.StartHost(NetworkData.Get()._gamePort);
        }
    }
}