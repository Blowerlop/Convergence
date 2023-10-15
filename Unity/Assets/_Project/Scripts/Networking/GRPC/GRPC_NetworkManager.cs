using System;
using System.Collections.Generic;
using GRPCClient;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(GRPC_Transport))]
    [DisallowMultipleComponent]
    public class GRPC_NetworkManager : MonoSingleton<GRPC_NetworkManager>
    {
        private MainService.MainServiceClient _client => networkTransport.client;
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

        private readonly List<GRPC_NetworkBehaviour> _networkBehaviours = new List<GRPC_NetworkBehaviour>();

        // public readonly Event onClientStartEvent = new Event(nameof(onClientStartEvent));
        public readonly Event onClientStartedEvent = new Event(nameof(onClientStartedEvent));

        public Event onClientEndEvent => networkTransport.onClientEndEvent;
        public readonly Event onClientEndedEvent = new Event(nameof(onClientStartedEvent));

        protected override void Awake()
        {
            networkTransport = GetComponent<GRPC_Transport>();
        }

        private void Start()
        {
            // onClientPreEndedEvent = networkTransport.onClientPreEndedEvent;
        }
        
        [Button]
        public async void StartClient()
        {
            bool connectionState = await networkTransport.StartClient();
            if (connectionState)
            {
                onClientStartedEvent.Invoke(this, true);
            }
        }

        [Button]
        public void StopClient()
        {
            if (networkTransport.StopClient())
            {
                DisposeClients();
                
                onClientEndedEvent.Invoke(this, true);
            }
        }

        public void Register(GRPC_NetworkBehaviour networkBehaviour)
        {
            if (_networkBehaviours.Contains(networkBehaviour)) return;
            
            _networkBehaviours.Add(networkBehaviour);
        }

        public void Unregister(GRPC_NetworkBehaviour networkBehaviour)
        {
            if (_networkBehaviours.Contains(networkBehaviour) == false) return;
            
            _networkBehaviours.Remove(networkBehaviour);
        }
        
        private void DisposeClients()
        {
            for (int i = 0; i < _networkBehaviours.Count; i++)
            {
                _networkBehaviours[i].Dispose();
            }
        }

        public void ClientsCancelTokens()
        {
            for (int i = 0; i < _networkBehaviours.Count; i++)
            {
                _networkBehaviours[i].CleanTokens();
            }
        }
    }
}
