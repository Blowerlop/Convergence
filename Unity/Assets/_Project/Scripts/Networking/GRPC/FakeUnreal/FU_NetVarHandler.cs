using System;
using System.Collections.Generic;
using System.Threading;
using Grpc.Core;
using GRPCClient;

namespace Project
{
    public class FU_NetVarHandler : MonoSingleton<FU_NetVarHandler>
    {
        private readonly Dictionary<GRPC_GenericType, AsyncServerStreamingCall<GRPC_NetVarUpdate>> _netVarUpdateStreams = new();
        private readonly Dictionary<GRPC_GenericType, Action<GRPC_NetVarUpdate>> _netVarUpdateEvents = new();
        
        private CancellationTokenSource _readStreamCancellationTokenSource = new();

        private void Start()
        {
            FU_GRPC_NetworkManager.instance.onClientStopEvent += OnClientStop;
        }

        public void TryCreateStream(GRPC_GenericType type)
        {
            if (_netVarUpdateStreams.ContainsKey(type)) return;
            
            var newStream =
                FU_GRPC_Transport.instance.client.GRPC_CliNetNetVarUpdate(new GRPC_GenericValue { Type = type });
                
            _netVarUpdateStreams.Add(type, newStream);

            ReadValues(type);
        }

        public Action<GRPC_NetVarUpdate> GetEvent(GRPC_GenericType type)
        {
            if (_netVarUpdateEvents.TryGetValue(type, out var eventVar))
            {
                return eventVar;
            }

            Action<GRPC_NetVarUpdate> newEvent = default; 
            
            _netVarUpdateEvents.Add(type, newEvent);
            
            return newEvent;
        }
        
        private async void ReadValues(GRPC_GenericType type)
        {
            var stream = _netVarUpdateStreams[type];
            
            try
            {
                while (await stream.ResponseStream.MoveNext(_readStreamCancellationTokenSource.Token))
                {
                    _netVarUpdateEvents[type]?.Invoke(stream.ResponseStream.Current);
                }
            }
            catch (RpcException)
            {
                if(FU_GRPC_NetworkManager.instance.isConnected)
                    FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private void OnClientStop()
        {
            _readStreamCancellationTokenSource?.Cancel();
            _readStreamCancellationTokenSource?.Dispose();
            _readStreamCancellationTokenSource = null;
            
            foreach (var stream in _netVarUpdateStreams.Values)
            {
                stream?.Dispose();
            }

            _netVarUpdateStreams.Clear();
            _netVarUpdateEvents.Clear();
            
            if (FU_GRPC_NetworkManager.IsInstanceAlive())
            {
                FU_GRPC_NetworkManager.instance.onClientStopEvent -= OnClientStop;
            }
        }
    }
}