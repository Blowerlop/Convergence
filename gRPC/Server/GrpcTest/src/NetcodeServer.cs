using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public class NetcodeServer : GRPCClient
    {
        public Dictionary<int, NetworkObject> NetObjs = new();
        public IServerStreamWriter<GRPC_ClientUpdate> ClientUpdateStream = null!;

        public List<IServerStreamWriter<GRPC_TeamResponse>> teamSelectionResponseStream = new List<IServerStreamWriter<GRPC_TeamResponse>>();

        public NetcodeServer(string ad) : base(ad) { }

        public override void Disconnect()
        {
            base.Disconnect();
            MainServiceImpl.netcodeServer = null;
        }

        public override void Dispose()
        {
            
        }

        public void HandleNetObjUpdate(GRPC_NetObjUpdate update)
        {
            switch (update.Type)
            {
                case GRPC_NetObjUpdateType.New:
                    NewNetObject(update);
                    break;
                case GRPC_NetObjUpdateType.Destroy:
                    DestroyNetObject(update);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void NewNetObject(GRPC_NetObjUpdate update)
        {
            Console.WriteLine("New NetworkObject: NetID: " + update.NetId + ", PrefabID: " + update.PrefabId + "\n");
            NetObjs.Add(update.NetId, new NetworkObject(update.NetId, update.PrefabId));
        }
        
        private void DestroyNetObject(GRPC_NetObjUpdate update)
        {
            Console.WriteLine("Destroy NetworkObject: NetID: " + update.NetId + "\n");
            NetObjs.Remove(update.NetId);
        }

        public List<GRPC_NetObjUpdate> GetNetworkObjectsAsUpdates()
        {
            List<GRPC_NetObjUpdate> list = new();
            
            foreach (var netObj in NetObjs.Values)
            {
                list.Add(new GRPC_NetObjUpdate { NetId = netObj.NetId, Type = GRPC_NetObjUpdateType.New, PrefabId = netObj.PrefabId });
            }

            return list;
        }

        public List<GRPC_NetVarUpdate> GetNetworkVariablesAsUpdates()
        {
            List<GRPC_NetVarUpdate> list = new();

            foreach (var netObj in NetObjs.Values)
            {
                foreach (var netVar in netObj.NetVars)
                {
                    list.Add(new GRPC_NetVarUpdate { NetId = netObj.NetId, HashName = netVar.Key, NewValue = { Type = netVar.Value.Type, Value = netVar.Value.Value } });
                }
            }

            return list;
        }
    }
}
