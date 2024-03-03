using GRPCServer;

namespace Networking
{
    public class NetworkObject
    {
        public readonly int NetId;
        public readonly string PrefabId;
        
        public Dictionary<int, GRPC_GenericValue> NetVars = new();
        
        public static readonly object Locker = new object();

        public NetworkObject(int netId, string prefabId)
        {
            NetId = netId;
            PrefabId = prefabId;
        }
    }
}
