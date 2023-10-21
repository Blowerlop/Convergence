using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    public class GRPC_NetObjectsHandler : MonoSingleton<GRPC_NetObjectsHandler>, IDisposable
    {
        public GameObject cubePrefab;
        public GameObject playerPrefab;
        
        private readonly CancellationTokenSource _netObjsStreamCancelSrc = new CancellationTokenSource();
        private AsyncClientStreamingCall<GRPC_NetObjUpdate, GRPC_EmptyMsg> _netObjsStream;
        
        private void OnEnable()
        {
            GRPC_Transport.instance.onClientEndEvent.Subscribe(this, TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, GetNetObjsUpdateStream);
            GRPC_NetworkManager.instance.onClientEndedEvent.Subscribe(this, Dispose);
        }

        private void OnDisable()
        {
            if (GRPC_NetworkManager.isBeingDestroyed) return;
            
            GRPC_Transport.instance.onClientEndEvent.Unsubscribe(TokenCancel);
            GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(GetNetObjsUpdateStream);
            GRPC_NetworkManager.instance.onClientEndedEvent.Unsubscribe(Dispose);
        }

        private void GetNetObjsUpdateStream()
        {
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
    }
}
