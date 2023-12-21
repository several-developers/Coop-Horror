using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace NetcodePlus
{
    /// <summary>
    /// This authenticator is the base auth for Unity Services
    /// It will login in anonymous mode
    /// It is ideal for quick testing since it will skip login UI and create a temporary user.
    /// </summary>

    public class AuthenticatorUnity : Authenticator
    {
        public override async Task Initialize()
        {
            await base.Initialize();
            if(UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
        }

        public override async Task<bool> Login()
        {
            if (IsConnected())
                return true; //Already connected

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                _userID = AuthenticationService.Instance.PlayerId;
                if (_username == null)
                    _username = _userID;
                Debug.Log("Unity Auth: " + _userID + " " + _username);
                return true;
            }
            catch (AuthenticationException ex) { Debug.LogException(ex); }
            catch (RequestFailedException ex) { Debug.LogException(ex); }
            return false;
        }

        public override async Task<bool> Login(string username)
        {
            this._username = username;
            return await Login();
        }

        public override void Logout()
        {
            try
            {
                AuthenticationService.Instance.SignOut(true);
                _userID = null;
                _username = null;
            }
            catch (System.Exception) { }
        }

        public override bool IsConnected()
        {
            return AuthenticationService.Instance.IsAuthorized;
        }

        public override bool IsSignedIn()
        {
            return AuthenticationService.Instance.IsSignedIn;
        }

        public override bool IsExpired()
        {
            return AuthenticationService.Instance.IsExpired;
        }

        public override bool IsUnityServices()
        {
            return true;
        }

        public override string GetUsername()
        {
            return _username;
        }

        public override string GetUserId()
        {
            return _userID;
        }
    }
}
