using System.Threading.Tasks;

namespace NetcodePlus
{
    /// <summary>
    /// Base class for all Authenticators, must be inherited
    /// Note: Steam and Google Authenticator are just code examples and are not tested/implemented
    /// </summary>

    public abstract class Authenticator
    {
        // PROPERTIES: ----------------------------------------------------------------------------
        
        public string UserID => GetUserId();
        public string Username => GetUsername();

        // FIELDS: --------------------------------------------------------------------------------
        
        protected string _userID;
        protected string _username;
        protected bool _loggedIn;
        protected bool _inited;

        // PUBLIC METHODS: ------------------------------------------------------------------------
        
        public virtual async Task Initialize()
        {
            _inited = true;
            await Task.Yield(); //Do nothing
        }

        public virtual async Task<bool> Login()
        {
            await Task.Yield(); //Do nothing
            return false;
        }

        public virtual async Task<bool> Login(string username) =>
            await Login(); //Some authenticator dont define this function

        public virtual async Task<bool> Login(string username, string token) =>
            await Login(username); //Some authenticator dont define this function

        public virtual async Task<bool> RefreshLogin() =>
            await Login(); //Same as Login if not defined

        public virtual void LoginTest() => LoginTest(username: NetworkTool.GenerateRandomID());

        //Bypass login system by just assigning your own values, for testing
        public virtual void LoginTest(string username)
        {
            _userID = username;
            _username = username;
            _loggedIn = true;
        }

        public virtual async Task<bool> Register(string username) =>
            await Login(username); //Some authenticator dont define this function

        public virtual async Task<bool> Register(string username, string token) =>
            await Login(username, token); //Some authenticator dont define this function

        public virtual async Task<bool> Register(string username, string email, string token) =>
            await Login(username, token); //Some authenticator dont define this function

        public virtual async void Update(float delta) =>
            await Task.Yield();

        public virtual void Logout()
        {
            _loggedIn = false;
            _userID = null;
            _username = null;
        }

        public virtual bool IsInited() => _inited;

        public virtual bool IsConnected() =>
            IsSignedIn() && !IsExpired();

        public virtual bool IsSignedIn() => _loggedIn; //IsSignedIn will still be true if the login expires

        public virtual bool IsExpired() => false;

        public virtual bool IsUnityServices() => false; //Override condition

        public virtual string GetUserId() => _userID;

        public virtual string GetUsername() => _username;

        public virtual string GetProviderId() => GetUserId(); //By default, its same than user id

        public virtual string GetError() => ""; //Should return the latest error

        public static Authenticator Create(AuthenticatorType type)
        {
#if USER_LOGIN
            if (type == AuthenticatorType.UserLoginAPI)
                return new AuthenticatorUserApi();
#endif

            return type switch
            {
                AuthenticatorType.Google => new AuthenticatorGoogle(),
                AuthenticatorType.Steam => new AuthenticatorSteam(),
                AuthenticatorType.Unity => new AuthenticatorUnity(),
                _ => new AuthenticatorTest()
            };
        }

        public static Authenticator Get() =>
            TheNetwork.Get().Auth; //Access authenticator
    }

    public enum AuthenticatorType
    {
        Test = 0, //Ideal for quick testing, will just generate random ID, requires no integration

        Unity = 5,          //Ideal for testing Unity Services, will require to have Unity Services project ID linked to your unity project
        Google = 10,        //Not implemented
        Steam = 20,         //Not implemented

        UserLoginAPI = 80,  //3rd Party Asset required

        Custom = 100,       //Not implemented
    }
}