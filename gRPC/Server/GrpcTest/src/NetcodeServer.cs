namespace Networking
{
    public class NetcodeServer : GRPCClient
    {
        public Dictionary<int, NetworkObject> NetObjs = new();

        public NetcodeServer(string ad) : base(ad) { }
    }
}
