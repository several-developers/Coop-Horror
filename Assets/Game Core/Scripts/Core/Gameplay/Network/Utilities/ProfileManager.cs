using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GameCore.Gameplay.Network
{
    public class ProfileManager
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public string Profile
        {
            get => _profile ??= GetProfile();
            set
            {
                _profile = value;
                OnProfileChangedEvent?.Invoke();
            }
        }

        // FIELDS: --------------------------------------------------------------------------------
        
        public event Action OnProfileChangedEvent;

        private string _profile;

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private static string GetProfile()
        {
            return "Vladeski";
#if UNITY_EDITOR

            // When running in the Editor make a unique ID from the Application.dataPath.
            // This will work for cloning projects manually, or with Virtual Projects.
            // Since only a single instance of the Editor can be open for a specific
            // dataPath, uniqueness is ensured.
            byte[] hashedBytes = new MD5CryptoServiceProvider()
                .ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
            
            Array.Resize(ref hashedBytes, newSize: 16);
            return new Guid(hashedBytes).ToString("N");
#else
            return "";
#endif
        }
    }
}