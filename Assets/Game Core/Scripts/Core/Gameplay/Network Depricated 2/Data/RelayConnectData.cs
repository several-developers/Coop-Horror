namespace GameCore.Gameplay.NetworkDepricated2
{
    public class RelayConnectData
    {
        public string url;
        public ushort port;
        public byte[] alloc_id;
        public byte[] alloc_key;
        public byte[] connect_data;
        public byte[] host_connect_data;
        public string join_code;
    }
}