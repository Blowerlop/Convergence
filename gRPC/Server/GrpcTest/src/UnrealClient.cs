using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public class UnrealClient : GRPCClient
    {
        public readonly Dictionary<GRPC_GenericType, IServerStreamWriter<GRPC_NetVarUpdate>> netVarStream = new Dictionary<GRPC_GenericType, IServerStreamWriter<GRPC_NetVarUpdate>>();

        private IServerStreamWriter<GRPC_NetObjUpdate> _netObjectsStream; 
        public IServerStreamWriter<GRPC_NetObjUpdate> NetObjectsStream
        {
            get => _netObjectsStream;
            set
            {
                _netObjectsStream = value;
                if (value != null!) SendQueuedNetObjUpdates();
            }
        }
        
        //Stream is not created instantly when client connects, so we need to queue the updates while the stream is not set
        private readonly Queue<GRPC_NetObjUpdate> _netObjUpdateInWaiting = new();

        public static IServerStreamWriter<GRPC_Team> teamSelectionResponseStream;

        public UnrealClient(string ad) : base(ad) { }
        
        public override void Disconnect()
        {
            base.Disconnect();
            MainServiceImpl.unrealClients.Remove(Adress);
        }

        public override void Dispose()
        {
            
        }
        
        private async void SendQueuedNetObjUpdates()
        {
            while (_netObjUpdateInWaiting.Count > 0)
            {
                var update = _netObjUpdateInWaiting.Dequeue();
                await NetObjectsStream.WriteAsync(update);
                Console.WriteLine($"Deuqued update sent to {Adress} > {update.PrefabId}");
            }
        }
        
        public void QueueNetObjUpdate(GRPC_NetObjUpdate update)
        {
            _netObjUpdateInWaiting.Enqueue(update);
        }
    }
}
