using System;
using System.Collections.Generic;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(FU_GRPC_Transport))]
    [RequireComponent(typeof(FU_NetworkObjectManager))]
    [DisallowMultipleComponent]
    public class FU_GRPC_NetworkManager : MonoSingleton<FU_GRPC_NetworkManager>
    {
        public MainService.MainServiceClient client => networkTransport.client;
        [ShowInInspector] public FU_GRPC_Transport networkTransport { get; private set; }
        [ShowInInspector]
        public bool isConnected
        {
            get
            {
#if UNITY_EDITOR
                if (networkTransport == null && Application.isPlaying == false)
                {
                    networkTransport = GetComponent<FU_GRPC_Transport>();
                }

                if (networkTransport == null) return false;
                return networkTransport.isConnected;
#else
                return networkTransport.isConnected;
#endif
            }
        }
        
        public Event onClientStartedEvent = new Event(nameof(onClientStartedEvent));
        public Event onClientStopEvent => networkTransport.onClientStopEvent;
        public Event onClientEndedEvent = new Event(nameof(onClientEndedEvent));


        protected override void Awake()
        {
            networkTransport = GetComponent<FU_GRPC_Transport>();
        }

        [Button]
        public async void StartClient()
        {
            bool connectionState = await networkTransport.StartClient();
            if (connectionState)
            {
                //Instantiate(player);
                onClientStartedEvent.Invoke(this, true);
            }
        }

        [Button]
        public void StopClient()
        {
            if (networkTransport.StopClient())
            {
                onClientEndedEvent.Invoke(this, true);
            }
        }
    }
}