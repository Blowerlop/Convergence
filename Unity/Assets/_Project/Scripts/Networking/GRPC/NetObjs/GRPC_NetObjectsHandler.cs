using System;
using System.Collections.Generic;
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
        private AsyncDuplexStreamingCall<GRPC_NetObjUpdate, GRPC_EmptyMsg> _netObjsStream;
        
        private CancellationTokenSource _netObjsStreamCallbackCancelSrc;
        private AsyncServerStreamingCall<GRPC_NetObjUpdate> _netObjsCallbackStream;
        
        public Dictionary<int, GRPC_NetworkObjectSyncer> _processNetObjs = new Dictionary<int, GRPC_NetworkObjectSyncer>();
        
        private void OnEnable()
        {
            GRPC_Transport.instance.onClientStopEvent += TokenCancel;
            GRPC_NetworkManager.instance.onClientStartedEvent += GetNetObjsUpdateStream;
            GRPC_NetworkManager.instance.onClientStartedEvent += GetNetObjsUpdate;
            GRPC_NetworkManager.instance.onClientStoppedEvent += Dispose;
        }

        private void OnDisable()
        {
            if (GRPC_NetworkManager.IsInstanceAlive() == false) return;
            
            GRPC_Transport.instance.onClientStopEvent -= TokenCancel;
            GRPC_NetworkManager.instance.onClientStartedEvent -= GetNetObjsUpdateStream;
            GRPC_NetworkManager.instance.onClientStartedEvent -= GetNetObjsUpdate;
            GRPC_NetworkManager.instance.onClientStoppedEvent -= Dispose;
        }

        private void GetNetObjsUpdateStream()
        {
            _netObjsStreamCancelSrc = new CancellationTokenSource();
            _netObjsStream = GRPC_Transport.instance.client.GRPC_SrvNetObjUpdate();

            _netObjsStreamCallbackCancelSrc = new CancellationTokenSource();
            _netObjsCallbackStream = GRPC_Transport.instance.client.GRPC_CliNetObjUpdate(new GRPC_EmptyMsg());
        }
        
        public void SendNetObjsUpdate(GRPC_NetObjUpdate update, GRPC_NetworkObjectSyncer networkObjectSyncer)
        {
            try
            {
                networkObjectSyncer.hasBeenSpawnedOnGrpc = false;
                
                GRPC_NetworkLoop.instance.AddMessage(new GRPC_Message<GRPC_NetObjUpdate>(_netObjsStream.RequestStream, update, new CancellationTokenSource()));
                _processNetObjs.Add(update.NetId, networkObjectSyncer);
            }
            catch (IOException)
            {
                if (GRPC_NetworkManager.instance.isConnected)
                    GRPC_NetworkManager.instance.StopClient();
            }
        }

        private async void GetNetObjsUpdate()
        {
            try
            {
                while (await _netObjsCallbackStream.ResponseStream.MoveNext(_netObjsStreamCallbackCancelSrc.Token))
                {
                    GRPC_NetObjUpdate response = _netObjsCallbackStream.ResponseStream.Current;
                    GRPC_NetworkObjectSyncer networkObjectSyncer = _processNetObjs[response.NetId];
                    networkObjectSyncer.hasBeenSpawnedOnGrpc = true;
                    networkObjectSyncer.onNetworkObjectHasSpawnedOnGrpc?.Invoke();
                    _processNetObjs.Remove(response.NetId);
                }
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
            _netObjsStreamCallbackCancelSrc?.Cancel();
        }
        
        public void Dispose()
        {            
            _netObjsStreamCancelSrc?.Dispose();
            _netObjsStream?.Dispose();

            _netObjsStream = null;
            
            _netObjsStreamCallbackCancelSrc?.Dispose();
            _netObjsCallbackStream?.Dispose();

            _netObjsCallbackStream = null;
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
