using System;
using System.IO;
using System.Linq;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using Debug = UnityEngine.Debug;

namespace Project
{
    public class GRPC_NetObjectsHandler : MonoSingleton<GRPC_NetObjectsHandler>, IDisposable
    {
        public NetworkObject cubePrefab;
        public NetworkObject playerPrefab;

        private CancellationTokenSource _netObjsStreamCancelSrc;
        private AsyncClientStreamingCall<GRPC_NetObjUpdate, GRPC_EmptyMsg> _netObjsStream;
        
        private void OnEnable()
        {
            GRPC_Transport.instance.onClientStopEvent.Subscribe(this, TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, GetNetObjsUpdateStream);
            GRPC_NetworkManager.instance.onClientStoppedEvent.Subscribe(this, Dispose);
        }

        private void OnDisable()
        {
            if (GRPC_NetworkManager.IsInstanceAlive() == false) return;
            
            GRPC_Transport.instance.onClientStopEvent.Unsubscribe(TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(GetNetObjsUpdateStream);
            GRPC_NetworkManager.instance.onClientStoppedEvent.Unsubscribe(Dispose);
        }

        private void GetNetObjsUpdateStream()
        {
            _netObjsStreamCancelSrc = new CancellationTokenSource();
            _netObjsStream = GRPC_Transport.instance.client.GRPC_SrvNetObjUpdate();
        }
        
        public async void SendNetObjsUpdate(GRPC_NetObjUpdate update)
        {
            if (_netObjsStream == null) return;
            
            try
            {
                await _netObjsStream.RequestStream.WriteAsync(update);
            }
            catch (IOException)
            {
                if (GRPC_NetworkManager.instance.isConnected)
                    GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private void TokenCancel()
        {
            _netObjsStreamCancelSrc?.Cancel();
        }
        
        public void Dispose()
        {            
            _netObjsStreamCancelSrc?.Dispose();
            _netObjsStream?.Dispose();

            _netObjsStream = null;
        }
        
        #region Debug
        
        [ConsoleCommand("debug_spawn", "Spawn a dummy network object to test sync between Unreal and Unity.")]
        public static void DbgSpawnCmd(string name)
        {
            NetworkObject prefab = null;
            
            switch (name.ToLower())
            {
                case "player":
                    prefab = instance.playerPrefab;
                    break;
                case "cube":
                    prefab = instance.cubePrefab;
                    break;
                default:
                    prefab = instance.cubePrefab;
                    break;
            }
            
            Instantiate(prefab).Spawn();
        }
        
        [ConsoleCommand("debug_spawn_unreal_ownership", "Spawn a dummy network object to test sync between Unreal and Unity.")]
        public static void DbgSpawnWithUnrealOwnershipCmd(string name, string address)
        {
            NetworkObject prefab = null;
            
            switch (name.ToLower())
            {
                case "player":
                    prefab = instance.playerPrefab;
                    break;
                case "cube":
                    prefab = instance.cubePrefab;
                    break;
                default:
                    prefab = instance.cubePrefab;
                    break;
            }
            
            Instantiate(prefab).SpawnWithUnrealOwnership(address);
        }
        
        [ConsoleCommand("debug_despawn", 
            "Despawn random network object to test sync between Unreal and Unity.")]
        public static void DbgDespawnCmd(int netId)
        {
            var obj =
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values.FirstOrDefault(
                    x => x.NetworkObjectId == (ulong)netId);

            if (obj == null)
            {
                Debug.LogError($"There are no spawned objects that correspond to netId: {netId}");
                return;
            }
            
            obj.Despawn();
        }
        
        #endregion
    }
}
