using System;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(GRPC_Transport))]
    [DisallowMultipleComponent]
    public class GRPC_NetworkManager : Singleton<GRPC_NetworkManager>
    {
        private MainService.MainServiceClient _client;
        [ShowInInspector] public GRPC_Transport networkTransport { get; private set; }
        [ShowInInspector]
        public bool isConnected
        {
            get
            {
                #if UNITY_EDITOR
                if (Application.isPlaying == false && networkTransport == null)
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

        protected override void Awake()
        {
            networkTransport = GetComponent<GRPC_Transport>();
        }

        public void StartClient()
        {
            networkTransport.StartClient();
        }
    }
}
