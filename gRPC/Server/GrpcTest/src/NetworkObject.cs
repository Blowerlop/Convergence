namespace Networking
{
    public class NetworkObject
    {
        public readonly int NetId;
        public readonly string PrefabId;
        
        public Dictionary<int, string> NetVars = new();

        public NetworkObject(int netId, string prefabId)
        {
            NetId = netId;
            PrefabId = prefabId;
        }
    }
}
