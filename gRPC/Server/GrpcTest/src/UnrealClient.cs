using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public readonly Dictionary<int, IServerStreamWriter<GRPC_NetVarUpdate>> netVarStream = new Dictionary<int, IServerStreamWriter<GRPC_NetVarUpdate>>();


        public UnrealClient(string ad) : base(ad) { }

        
        public override void Disconnect()
        {
            base.Disconnect();
            MainServiceImpl.unrealClients.Remove(Adress);
        }

        public override void Dispose()
        {
            
        }
    }
}
