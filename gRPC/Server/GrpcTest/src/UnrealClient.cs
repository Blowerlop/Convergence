using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public readonly Dictionary<GRPC_GenericType, IServerStreamWriter<GRPC_NetVarUpdate>> netVarStream = new Dictionary<GRPC_GenericType, IServerStreamWriter<GRPC_NetVarUpdate>>();
        public IServerStreamWriter<GRPC_NetObjUpdate> NetObjectsStream;

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
