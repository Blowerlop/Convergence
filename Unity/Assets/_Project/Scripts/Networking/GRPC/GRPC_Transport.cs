using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;
using BestHTTP.Logger;
using Grpc.Core;
using GRPC.NET;
using Grpc.Net.Client;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(GRPC_NetworkManager))]
    [DisallowMultipleComponent]
    public class GRPC_Transport : MonoSingleton<GRPC_Transport>
    {
        [SerializeField] private string _address = "127.0.0.1";
        [SerializeField] private ushort _port = 5001;
        public bool isConnected { get; private set; }
        private GrpcChannel _channel;
        public MainService.MainServiceClient client { get; private set; }
        
        // public readonly Event onClientStartEvent = new Event(nameof(onClientStartEvent));
        public readonly Event onClientStopEvent = new Event(nameof(onClientStopEvent));
        
        private void Start()
        {
            HTTPManager.Logger.Level = Loglevels.None;
        }

        
        /// <summary>
        /// Do not call directly this method. Use GRPC_NetworkManager.StartClient() instead.
        /// </summary>
        public async Task<bool> StartClient()
        {
            if (isConnected)
            {
                Debug.LogWarning("Can't start a client when there is one already running");
                return false;
            }
            
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif

            HTTPManager.Setup();
            GRPCBestHttpHandler httpHandler = new GRPCBestHttpHandler();

            _channel = GrpcChannel.ForAddress($"https://{_address}:{_port}", new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            client = new MainService.MainServiceClient(_channel);
            
            isConnected = await NHandshake();
            
            return isConnected;
        }

        /// <summary>
        /// Do not call directly this method. Use GRPC_NetworkManager.StopClient() instead.
        /// </summary>
        public bool StopClient()
        {
            if (!isConnected)
            {
                Debug.LogWarning("No client are running");
                return false;
            }
            
            Debug.Log("Connection shutting down... cleaning client");
            onClientStopEvent.Invoke(this, true);
            
            // To do: find a better place to put this
            GRPC_NetworkManager.instance.ClientsCancelTokens();

            _channel.ShutdownAsync().Wait();
            _channel = null;

            isConnected = false;
            Debug.Log("Client cleaned !");
            return true;
        }
        
        #region Handshake
        
        async Task<bool> NHandshake()
        {
            Debug.Log("GRPCClient.cs > NHandshake...");

            GRPC_NHandshakeGet response;
            try
            {
                response = await client.GRPC_NetcodeHandshakeAsync(new GRPC_NHandshakePost());
                Debug.Log($"GRPCClient.cs > NHandshake result: {response.Result}");
            }
            catch (RpcException e)
            {
                response = new GRPC_NHandshakeGet(new GRPC_NHandshakeGet() { Result = -1 });
                Debug.LogError($"GRPCClient.cs > Connection failed : {e}");
            }
            
            
            return response.Result == 0;
        }
        #endregion
    }
}