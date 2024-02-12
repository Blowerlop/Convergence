using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    public class FU_GRPC_Rtt : MonoSingleton<FU_GRPC_Rtt>, IDisposable
    {
        //Ping
        private readonly CancellationTokenSource _pingCancelSrc = new CancellationTokenSource();
        private AsyncDuplexStreamingCall<GRPC_PingPost, GRPC_PingGet> _pingStream;

        public float currentRTT { get; private set; }

        private float _start = 0.0f;
        private float _end = 0.0f;
        [SerializeField] private float _heartBeat = 0.5f;
        public Event<float> onRttUpdateEvent = new Event<float>(nameof(onRttUpdateEvent));

        private void OnEnable()
        {
            FU_GRPC_NetworkManager.instance.onClientEndedEvent.Subscribe(this, Dispose);
        }

        private void OnDisable()
        {
            if (FU_GRPC_NetworkManager.IsInstanceAlive())
            {
                FU_GRPC_NetworkManager.instance.onClientEndedEvent.Unsubscribe(Dispose);
            }
        }

        private void FixedUpdate()
        {
            // if (first == false) return;
            if (FU_GRPC_Transport.instance.isConnected == false)
            {
                currentRTT = -1;
                return;
            }
            
            if (_pingStream == null)
            {
                _pingStream = FU_GRPC_Transport.instance.client.GRPC_Ping();
                if (_pingStream == null) return;
                PingGetClientRpc();
            }
            
            if (Time.realtimeSinceStartup - _start > _heartBeat)
            {
                currentRTT = Mathf.Round((_end - _start) * 1000);
                Debug.Log("Current RTT : " + currentRTT);
                _start = Time.realtimeSinceStartup;
                Debug.Log("Ping : " + _start);
                onRttUpdateEvent.Invoke(this, false, currentRTT);
                PingPostServerRpc();
            }
            
        }
        
        private async void PingPostServerRpc()
        {
            try
            {
                await _pingStream.RequestStream.WriteAsync(new GRPC_PingPost(), _pingCancelSrc.Token);
            }
            catch (IOException)
            {
                FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private async void PingGetClientRpc()
        {
            try
            {
                while (await _pingStream.ResponseStream.MoveNext(_pingCancelSrc.Token))
                {
                    _end = Time.realtimeSinceStartup;
                    Debug.Log("Pong : " + _end);
                }
            }
            catch (RpcException)
            {
                FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        public void Dispose()
        {
            _pingCancelSrc?.Dispose();
            _pingStream?.Dispose();

            _pingStream = null;
        }
    }
}
