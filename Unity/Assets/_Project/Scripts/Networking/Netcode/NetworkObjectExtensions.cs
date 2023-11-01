using Unity.Netcode;

namespace Project
{
    public static class NetworkObjectExtensions
    {
        public static void SpawnWithUnrealOwnership(this NetworkObject obj, string address, bool destroyWithScene = false)
        {
            GRPC_NetworkManager.instance.GetUnrealClientByAddress(address).GiveOwnership(obj);
            obj.Spawn(destroyWithScene);
        }
        
        public static void SpawnWithUnrealOwnership(this NetworkObject obj, UnrealClient cli, bool destroyWithScene = false)
        {
            cli.GiveOwnership(obj);
            obj.Spawn(destroyWithScene);
        }
        
        public static void RemoveUnrealOwnership(this NetworkObject obj, string address)
        {
            GRPC_NetworkManager.instance.GetUnrealClientByAddress(address).RemoveOwnership(obj);
        }
        
        public static void RemoveUnrealOwnership(this NetworkObject obj, UnrealClient cli)
        {
            cli.RemoveOwnership(obj);
        }
        
        //Didn't find how to create a cache
        public static GRPC_NetworkObjectSyncer GetSyncer(this NetworkObject obj)
        {
            return obj.GetComponent<GRPC_NetworkObjectSyncer>();
        }
    }
}