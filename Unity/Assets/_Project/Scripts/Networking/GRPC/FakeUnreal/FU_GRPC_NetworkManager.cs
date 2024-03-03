using System;
using GRPCClient;
using Sirenix.OdinInspector;
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

        public event Action onClientStartedEvent;
        public event Action onClientStopEvent;
        public event Action onClientEndedEvent;


        protected override void Awake()
        {
            networkTransport = GetComponent<FU_GRPC_Transport>();
            networkTransport.onClientStopEvent += onClientStopEvent;
        }

        [Button]
        public async void StartClient()
        {
            bool connectionState = await networkTransport.StartClient();
            if (connectionState)
            {
                //Instantiate(player);
                onClientStartedEvent?.Invoke();
            }
        }

        [Button]
        public void StopClient()
        {
            if (networkTransport.StopClient())
            {
                onClientEndedEvent?.Invoke();
            }
        }
    }
}