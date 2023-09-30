using System;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(GRPC_Transport))]
    [DisallowMultipleComponent]
    public class GRPC_NetworkManager : MonoSingleton<GRPC_NetworkManager>
    {
        private MainService.MainServiceClient _client;
        [ShowInInspector] public GRPC_Transport networkTransport { get; private set; }
        [ShowInInspector]
        public bool isConnected
        {
            get
            {
                #if UNITY_EDITOR
                if (networkTransport == null && Application.isPlaying == false)
                {
                    networkTransport = GetComponent<GRPC_Transport>();
                }

                if (networkTransport == null) return false;
                return networkTransport.isConnected;
                #else
                return networkTransport.isConnected;
                #endif
            }
        }

        public Event onClientStartedEvent = new Event(nameof(onClientStartedEvent));
        public Event onClientEndedEvent = new Event(nameof(onClientStartedEvent));

        protected override void Awake()
        {
            networkTransport = GetComponent<GRPC_Transport>();
        }

        public async void StartClient()
        {
            bool connectionState = await networkTransport.StartClient();
            if (connectionState)
            {
                onClientStartedEvent.Invoke(this, true);
            }
        }

        public void StopClient()
        {
            if (networkTransport.StopClient())
            {
                onClientEndedEvent.Invoke(this, true);
            }
        }
    }
}
