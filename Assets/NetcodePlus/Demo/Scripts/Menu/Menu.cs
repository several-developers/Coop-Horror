using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace NetcodePlus.Demo
{

    public class Menu : MonoBehaviour
    {
        [Header("Panels")]
        public UIPanel main_panel;
        public UIPanel create_panel;
        public UIPanel load_panel;
        public UIPanel join_panel;

        [Header("Create")]
        public Text create_ip;
        public InputField create_user;
        public OptionSelectorInt create_mode;
        public OptionSelector create_color;

        [Header("Join")]
        public InputField join_user;
        public InputField join_host;
        public OptionSelector join_color;

        private ushort _port;

        public static string Username;
        public static string LastMenu = "Menu";

        private static Menu _instance;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            main_panel.Show();
            _port = NetworkData.Get()._gamePort;
            create_ip.text = "LAN IP: " + NetworkTool.GetLocalIp();
            LastMenu = SceneManager.GetActiveScene().name;

            LoadUser();
        }

        private void SaveUser(string user) =>
            Username = user;

        private void LoadUser()
        {
            string name = Menu.Username;
            if (name == null && DemoData.Get() != null)
                name = DemoData.Get().GetRandomName();
            if (name == null)
                name = "player";

            create_user.text = name;
            join_user.text = name;
        }

        public void OnClickGoToCreate()
        {
            main_panel.Hide();
            create_panel.Show();
        }

        public void OnClickGoToLoad()
        {
            main_panel.Hide();
            load_panel.Show();
        }

        public void OnClickGoToJoin()
        {
            main_panel.Hide();
            join_panel.Show();
        }

        public void OnClickGoToMain()
        {
            main_panel.Show();
            create_panel.Hide();
            load_panel.Hide();
            join_panel.Hide();
        }

        public void OnClickCreate()
        {
            GameMode mode = (GameMode) create_mode.GetSelectedValue();
            CreateGame(create_user.text, mode, create_color.GetSelectedValue());
        }

        public void OnClickJoin()
        {
            JoinGame(join_user.text, join_host.text, join_color.GetSelectedValue());
        }

        public void CreateGame(string user, GameMode mode, string character)
        {
            GameModeData mdata = GameModeData.Get(mode);
            
            if (SceneNav.DoSceneExist(mdata.Scene))
            {
                DemoConnectData cdata = new(mode);
                cdata.SetCharacter(character);
                TheNetwork.Get().SetConnectionExtraData(cdata);
                SaveUser(user);
                CreateTask(user, mdata.Scene);
            }
        }

        public void JoinGame(string user, string host, string character)
        {
            DemoConnectData cdata = new();
            cdata.SetCharacter(character);
            TheNetwork.Get().SetConnectionExtraData(cdata);
            SaveUser(user);
            JoinTask(user, host);
        }

        private async void CreateTask(string user, string scene)
        {
            TheNetwork.Get().Disconnect();
            BlackPanel.Get().Show();
            await Task.Yield(); //Wait a frame after the disconnect
            Authenticator.Get().LoginTest(user);
            TheNetwork.Get().StartHost(_port);
            TheNetwork.Get().LoadScene(scene);
        }

        private async void JoinTask(string user, string host)
        {
            TheNetwork.Get().Disconnect();
            ConnectingPanel.Get().Show();
            await Task.Yield(); //Wait a frame after the disconnect
            Authenticator.Get().LoginTest(user);
            TheNetwork.Get().StartClient(host, _port);
        }

        public static void GoToSimpleMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public static void GoToLobbyMenu()
        {
            SceneManager.LoadScene("MenuLobby");
        }

        public static void GoToLastMenu()
        {
            SceneManager.LoadScene(LastMenu);
        }

        public static Menu Get()
        {
            return _instance;
        }
    }
}
