using System;
using System.Collections.Generic;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Project
{
    [RequireComponent(typeof(FU_GRPC_Transport))]
    [RequireComponent(typeof(FU_NetworkObjectManager))]
    [DisallowMultipleComponent]
    public class FU_GRPC_NetworkManager : MonoSingleton<FU_GRPC_NetworkManager>
    {
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
        public Event onClientEndedEvent = new Event(nameof(onClientEndedEvent));

        protected override void Awake()
        {
            networkTransport = GetComponent<FU_GRPC_Transport>();
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