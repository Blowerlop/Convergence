using Grpc.Core;
using GRPCServer;
using GRPCServer.Services;

namespace Networking
{
    public class NetcodeServer : GRPCClient
    {
        public Dictionary<int, NetworkObject> NetObjs = new();
        public IServerStreamWriter<GRPC_ClientUpdate> ClientUpdateStream = null!;

        public IServerStreamWriter<GRPC_NetObjUpdate> NetObjectsStream;
        
        public List<IServerStreamWriter<GRPC_TeamResponse>> teamSelectionResponseStream = new List<IServerStreamWriter<GRPC_TeamResponse>>();
        public Dictionary<IServerStreamWriter<GRPC_NetVarUpdate>, GRPC_NetVarUpdate> requestNetvarUpdateStream = new Dictionary<IServerStreamWriter<GRPC_NetVarUpdate>, GRPC_NetVarUpdate>();

        public IServerStreamWriter<GRPC_SpellCastRequest>? SpellCastRequestStream = null;
        public IServerStreamWriter<GRPC_SpellSlot>? SetUnrealSpellStream = null;
        
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
            Debug.Log("New NetworkObject: NetID: " + update.NetId + ", PrefabID: " + update.PrefabId + "\n");
            NetObjs.Add(update.NetId, new NetworkObject(update.NetId, update.PrefabId));
        }
        
        private void DestroyNetObject(GRPC_NetObjUpdate update)
        {
            lock (NetObjs)
            {
                Debug.Log("Destroy NetworkObject: NetID: " + update.NetId + "\n");
                NetObjs.Remove(update.NetId);
            }
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

            foreach (NetworkObject netObj in NetObjs.Values)
            {
                foreach (KeyValuePair<int, GRPC_GenericValue> netVar in netObj.NetVars)
                {
                    if (netVar.Value == null) continue;

                    GRPC_GenericValue genericValue = new GRPC_GenericValue { Type = netVar.Value.Type, Value = netVar.Value.Value };
                    GRPC_NetVarUpdate update = new GRPC_NetVarUpdate { NetId = netObj.NetId, HashName = netVar.Key, NewValue = genericValue };
                    list.Add(update);

                }
            }

            return list;
        }
    }
}
