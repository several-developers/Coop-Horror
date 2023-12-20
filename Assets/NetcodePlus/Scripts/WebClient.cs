using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NetcodePlus
{

    public class WebClient : MonoBehaviour
    {
        private static WebClient _instance;

        private string _url;
        private ulong _clientID = 0;    //Will be sent as key
        private string _key;             //Custom key, will only be used if the client_id is 0

        void Awake()
        {
            _instance = this;
            _clientID = NetworkTool.GenerateRandomUInt64();
        }

        public void SetDefaultUrl(string host, ushort port, bool secured = false) =>
            _url = GetRawUrl(host, port, secured);

        public void SetDefaultUrlRaw(string url) =>
            _url = url;

        public void SetClientID(ulong client_id)
        {
            _clientID = client_id;
            _key = "";
        }

        public void SetKey(string key)
        {
            _key = key;
            _clientID = 0;
        }

        // ---------- Send Values --------------

        public async Task<WebResponse> Send(string path) =>
            await SendGetRequest(_url + "/" + path);

        public async Task<WebResponse> Send(string path, ulong data)
        {
            string jdata = data.ToString();
            return await SendPostRequest(_url + "/" + path, jdata);
        }

        public async Task<WebResponse> Send<T1>(string path, T1 data)
        {
            string jdata = WebTool.ToJson(data);
            return await SendPostRequest(_url + "/" + path, jdata);
        }

        // ---------- Send Values to raw URL --------------

        public async Task<WebResponse> SendUrl(string furl) =>
            await SendGetRequest(furl);

        public async Task<WebResponse> SendUrl(string furl, ulong data)
        {
            string jdata = data.ToString();
            return await SendPostRequest(furl, jdata);
        }

        public async Task<WebResponse> SendUrl<T1>(string furl, T1 data)
        {
            string jdata = WebTool.ToJson(data);
            return await SendPostRequest(furl, jdata);
        }

        // ---------- Requests --------------

        public async Task<WebResponse> SendGetRequest(string url) =>
            await SendRequest(url, WebRequest.METHOD_GET, "");

        public async Task<WebResponse> SendPostRequest(string url, string json_data) =>
            await SendRequest(url, WebRequest.METHOD_POST, json_data);

        public async Task<WebResponse> SendRequest(string url, string method, string json_data)
        {
            string akey = _clientID > 0 ? _clientID.ToString() : _key;
            UnityWebRequest request = WebRequest.Create(url, method, json_data, akey);
            return await WebTool.SendRequest(request);
        }

        public async Task<WebResponse> SendUploadRequest(string url, string path, byte[] data)
        {
            string akey = _clientID > 0 ? _clientID.ToString() : _key;
            UnityWebRequest request = WebRequest.CreateImageUploadForm(url, path, data, akey);
            return await WebTool.SendRequest(request);
        }

        // ---------- Getters --------------

        public ulong GetClientID() => _clientID;

        public string GetKey() => _key;

        public string GetRawUrl(string host, ushort port, bool secured = false)
        {
            string http = secured ? "https://" : "http://";
            return http + host + (port != 80 ? ":" + port : "");
        }

        public string GetRawUrl(string host, ushort port, string path, bool secured = false)
        {
            string http = secured ? "https://" : "http://";
            return http + host + (port != 80 ? ":" + port : "") + "/" + path;
        }

        public static WebClient Get() => _instance;
    }
}
