using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GRPCClient;
using UnityEngine;

namespace Project
{
    public class FU_NetworkObjectManager : MonoSingleton<FU_NetworkObjectManager>, IDisposable
    {
        private FU_GRPC_NetworkManager _networkManager;
        private MainService.MainServiceClient _client =>
            FU_GRPC_Transport.instance ? FU_GRPC_Transport.instance.client : null;

        private Dictionary<ulong, FU_NetworkObject> _networkObjects = new();
        
        //In Resources/_prefabPath
        [SerializeField] private string _prefabPath = "NetworkObjects/";
        
        //NetObjects Update
        private readonly CancellationTokenSource _netObjUpdateCancelSrc = new();
        private AsyncServerStreamingCall<GRPC_NetObjUpdate> _netObjUpdateStream;

        private void OnEnable()
        {
            _networkManager = FU_GRPC_NetworkManager.instance;
            
            _networkManager.onClientEndedEvent.Subscribe(this, Dispose);
            // _networkManager.onClientStartedEvent.Subscribe(this, StartNetObjsUpdateStream);
        }

        private void OnDisable()
        {
            if (FU_GRPC_NetworkManager.isBeingDestroyed) return;
            
            _networkManager.onClientEndedEvent.Unsubscribe(Dispose);
        }
        
        #region Stream

        //Can't subscribe directly to onClientStartedEvent with NetObsUpdateStream because return type is not void
        private void StartNetObjsUpdateStream()
        {
            if (_netObjUpdateStream != null)
            {
                Debug.LogWarning("Trying to start a NetObjectUpdateStream when there is one already running.");
                return;
            }
            
            NetObsUpdateStream();
        }
        
        private async Task NetObsUpdateStream()
        {
            _netObjUpdateStream = _client.GRPC_CliNetObjUpdate(new GRPC_EmptyMsg());

            try
            {
                while (await _netObjUpdateStream.ResponseStream.MoveNext(_netObjUpdateCancelSrc.Token))
                {
                    var update = _netObjUpdateStream.ResponseStream.Current;
                    ComputeNetObjUpdate(update);
                }
            }
            catch (RpcException)
            { 
                _networkManager.StopClient();
            }
        }
        
        #endregion
        
        #region Handle Update
        
        public void ComputeNetObjUpdate(GRPC_NetObjUpdate update)
        {
            switch (update.Type)
            {
                case GRPC_NetObjUpdateType.New:
                    HandleNewNetObj(update);
                    break;
                case GRPC_NetObjUpdateType.Destroy:
                    HandleDestroyNetObj(update);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(update),
                        "Received a NetObjUpdate with an unknown type.");
            }
        }

        private void HandleNewNetObj(GRPC_NetObjUpdate update)
        {
            if(_networkObjects.ContainsKey((ulong)update.NetId))
            {
                Debug.LogError("Trying to create a NetworkObject that already exists.");
                return;
            }

            var netObj = SpawnNetworkObject(update.PrefabId, (ulong)update.NetId);
                    
            _networkObjects.Add((ulong)update.NetId, netObj);
        }
        
        private void HandleDestroyNetObj(GRPC_NetObjUpdate update)
        {
            var netId = (ulong)update.NetId;
            
            if(!_networkObjects.ContainsKey(netId))
            {
                Debug.LogError("Trying to destroy a NetworkObject that doesn't exist.");
                return;
            }

            var netObj = _networkObjects[netId];
            
            Destroy(netObj.gameObject);
            
            _networkObjects.Remove(netId);
        }
        
        #endregion
        
        #region Spawn
        
        public FU_NetworkObject SpawnNetworkObject(string prefabId, ulong netId)
        {
            var prefab = Resources.Load<FU_NetworkObject>(_prefabPath + prefabId);
            var netObj = Instantiate(prefab);
            
            netObj.Init(netId);
            
            return netObj;
        }
        
        #endregion

        #region IDisposable
        
        public void Dispose()
        {
            //Need to cancel
            _netObjUpdateCancelSrc?.Dispose();
            _netObjUpdateStream?.Dispose();

            _netObjUpdateStream = null;
        }
        
        #endregion
    }
}