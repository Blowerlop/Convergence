using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    public class GRPC_Rtt : MonoSingleton<GRPC_Rtt>, IDisposable
    {
        //Ping
        private readonly CancellationTokenSource _pingCancelSrc = new CancellationTokenSource();
        private AsyncDuplexStreamingCall<GRPC_PingPost, GRPC_PingGet> _pingStream;

        [ShowInInspector, ReadOnly] public float currentRTT { get; private set; } = -1;

        private float _start = 0.0f;
        private float _end = 0.0f;
        [SerializeField] private float _heartBeat = 0.5f;
        public Event<float> onRttUpdateEvent = new Event<float>(nameof(onRttUpdateEvent));

        // private void Start()
        // {
        //     if (GRPC_Transport.instance.isConnected)
        //     {
        //         _pingStream = GRPC_Transport.instance.client.GRPC_Ping();
        //     }
        //     else
        //     {
        //         enabled = false;
        //     }
        // }

        private void OnEnable()
        {
            GRPC_Transport.instance.onClientPreEndedEvent.Subscribe(this, TokenCancel);
            GRPC_NetworkManager.instance.onClientEndedEvent.Subscribe(this, Dispose);
        }

        private void OnDisable()
        {
            if (GRPC_NetworkManager.isBeingDestroyed) return;
            
            GRPC_Transport.instance.onClientPreEndedEvent.Unsubscribe(TokenCancel);
            GRPC_NetworkManager.instance.onClientEndedEvent.Unsubscribe(Dispose);
        }

        private void FixedUpdate()
        {
            // if (first == false) return;
            if (GRPC_Transport.instance.isConnected == false)
            {
                currentRTT = -1;
                return;
            }
            
            if (_pingStream == null)
            {
                _pingStream = GRPC_Transport.instance.client.GRPC_Ping();
                if (_pingStream == null) return;
                PingGetClientRpc();
            }
            
            if (Time.realtimeSinceStartup - _start > _heartBeat)
            {
                currentRTT = Mathf.Round((_end - _start) * 1000);
                // Debug.Log("Current RTT : " + currentRTT);
                _start = Time.realtimeSinceStartup;
                // Debug.Log("Ping : " + _start);
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
                GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        private async void PingGetClientRpc()
        {
            try
            {
                while (await _pingStream.ResponseStream.MoveNext(_pingCancelSrc.Token))
                {
                    _end = Time.realtimeSinceStartup;
                    // Debug.Log("Pong : " + _end);
                }
            }
            catch (RpcException)
            {
                if (GRPC_NetworkManager.instance.isConnected)
                {
                    GRPC_NetworkManager.instance.StopClient();
                }
            }
        }

        private void TokenCancel()
        {
            _pingCancelSrc?.Cancel();
        }
        
        public void Dispose()
        {
            _pingCancelSrc?.Dispose();
            _pingStream?.Dispose();

            _pingStream = null;
        }
    }
}
