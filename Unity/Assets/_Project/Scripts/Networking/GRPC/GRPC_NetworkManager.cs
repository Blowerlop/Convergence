using System;
using System.Collections.Generic;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(GRPC_Transport))]
    [DisallowMultipleComponent]
    public class GRPC_NetworkManager : MonoSingleton<GRPC_NetworkManager>
    {
        public MainService.MainServiceClient client => networkTransport.client;
        [ShowInInspector] public GRPC_Transport networkTransport { get; private set; }
        [ShowInInspector]
        public bool isConnected
        {
            get
            {
                #if UNITY_EDITOR
                if (Application.isPlaying == false) return false;
                #endif
                
                if (networkTransport == null)
                {
                    if (TryGetComponent(out GRPC_Transport grpcTransport))
                    {
                        networkTransport = grpcTransport;
                        return grpcTransport.isConnected;
                    }

                    return false;
                }

                return networkTransport.isConnected;
            }
        }

        private readonly List<GRPC_NetworkBehaviour> _networkBehaviours = new List<GRPC_NetworkBehaviour>();
        
        //Unreal clients
        
        private readonly Dictionary<string, UnrealClient> _unrealClients = new();
        
        private CancellationTokenSource _unrealClientStreamCancelSrc = new CancellationTokenSource();
        private AsyncServerStreamingCall<GRPC_ClientUpdate> _unrealClientStream;
        
        //Events
        
        // public readonly Event onClientStartEvent = new Event(nameof(onClientStartEvent));
        public readonly Event onClientStartedEvent = new Event(nameof(onClientStartedEvent));

        public Event onClientStopEvent => networkTransport.onClientStopEvent;
        public readonly Event onClientStoppedEvent = new Event(nameof(onClientStoppedEvent));
        
        
        protected override void Awake()
        {
            networkTransport = GetComponent<GRPC_Transport>();
        }

        private void Reset()
        {
            networkTransport = GetComponent<GRPC_Transport>();
        }

        [Button]
        public async void StartClient()
        {
            bool connectionState = await networkTransport.StartClient();
            if (connectionState)
            {
                GetUnrealClientsUpdate();
                onClientStartedEvent.Invoke(this, true);
            }
        }

        [Button]
        public void StopClient()
        {
            if (networkTransport.StopClient())
            {
                DisposeClients();
                
                onClientStoppedEvent.Invoke(this, true);
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
            
            DisposeUnrealClientStream();
        }

        public void ClientsCancelTokens()
        {
            for (int i = 0; i < _networkBehaviours.Count; i++)
            {
                _networkBehaviours[i].CleanTokens();
            }

            CleanUnrealClientToken();
        }
        
        #region Unreal Clients

        private async void GetUnrealClientsUpdate()
        { 
            _unrealClientStream = client.GRPC_SrvClientUpdate(new GRPC_EmptyMsg());

            try
            {
                while (await _unrealClientStream.ResponseStream.MoveNext(_unrealClientStreamCancelSrc.Token))
                {
                    HandleUnrealClientUpdate(_unrealClientStream.ResponseStream.Current);
                }
            }
            catch (RpcException)
            {
                if (isConnected) StopClient();
            }
        }

        private void HandleUnrealClientUpdate(GRPC_ClientUpdate update)
        {
            switch (update.Type)
            {
                case GRPC_ClientUpdateType.Connect:
                    UnrealClientConnected(update.ClientIP);
                    break;
                case GRPC_ClientUpdateType.Disconnect:
                    UnrealClientDisconnected(update.ClientIP);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void UnrealClientConnected(string address)
        {
            if (_unrealClients.ContainsKey(address))
            {
                Debug.LogError($"Trying to connect an already connected unreal client {address}");
                return;
            }

            var cli = new UnrealClient(address);
            _unrealClients.Add(address, cli);

            if (NetworkManager.Singleton == null || NetworkManager.Singleton.NetworkConfig == null) return;

            var userInstance = NetworkManager.Singleton.NetworkConfig.PlayerPrefab.GetComponent<NetworkObject>();
            Instantiate(userInstance).SpawnWithUnrealOwnership(cli);
        }
        
        private void UnrealClientDisconnected(string address)
        {
            if (!_unrealClients.ContainsKey(address))
            {
                Debug.LogError($"Trying to disconnect an already disconnected unreal client {address}");
                return;
            }
            
            _unrealClients[address].Disconnect();
            _unrealClients.Remove(address);
        }

        private void DisposeUnrealClientStream()
        {
            _unrealClientStreamCancelSrc?.Dispose();
            _unrealClientStream?.Dispose();

            _unrealClientStream = null;
        }
        
        private void CleanUnrealClientToken()
        {
            _unrealClientStreamCancelSrc?.Cancel();
        }

        public UnrealClient GetUnrealClientByAddress(string address) =>
            !_unrealClients.ContainsKey(address) ? null : _unrealClients[address];

        [ConsoleCommand("display_unreal_clients", "Display the list of connected Unreal clients.")]
        public static void DisplayUnrealClientsCmd()
        {
            Debug.Log("Unreal Clients:");

            foreach (var client in instance._unrealClients)
            {
                Debug.Log(client.Key + "\n");
            }
        }
        
        #endregion
    }
}
