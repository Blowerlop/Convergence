using GRPCServer.Services;

namespace Networking
{
    public class NetcodeServer : GRPCClient
    {
        public Dictionary<int, NetworkObject> NetObjs = new();

        public NetcodeServer(string ad) : base(ad) { }


        public override void Disconnect()
        {
            base.Disconnect();
            MainServiceImpl.netcodeServer = null;
        }

        public override void Dispose()
        {
            
        }
    }
}
