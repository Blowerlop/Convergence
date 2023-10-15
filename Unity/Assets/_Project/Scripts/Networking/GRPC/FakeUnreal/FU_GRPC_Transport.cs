using System.Linq;
using System.Threading.Tasks;
using BestHTTP;
using BestHTTP.Logger;
using GRPC.NET;
using Grpc.Net.Client;
using GRPCClient;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    [RequireComponent(typeof(FU_GRPC_NetworkManager))]
    [DisallowMultipleComponent]
    public class FU_GRPC_Transport : MonoSingleton<FU_GRPC_Transport>
    {
        [SerializeField] private string _address = "127.0.0.1";
        [SerializeField] private ushort _port = 5001;
        public bool isConnected { get; private set; }
        private GrpcChannel _channel;
        public MainService.MainServiceClient client { get; private set; }
        
        public readonly Event onClientPreEndedEvent = new Event(nameof(onClientPreEndedEvent));
        
        private void Start()
        {
            HTTPManager.Logger.Level = Loglevels.None;
        }
        
        /// <summary>
        /// Do not call directly this method. Use GRPC_NetworkManager.StartClient()
        /// </summary>
        public async Task<bool> StartClient()
        {
            if (isConnected)
            {
                Debug.LogError("Can't start a client when there is one already running");
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
            
            isConnected = await Handshake();
            return isConnected;
        }

        /// <summary>
        /// Do not call directly this method. Use GRPC_NetworkManager.StopClient()
        /// </summary>
        public bool StopClient()
        {
            if (!isConnected)
            {
                Debug.LogError("No client are running");
                return false;
            }
            
            Debug.Log("Connection shutdown ! Cleaning client...");
            
            onClientPreEndedEvent.Invoke(this, false);
            
            _channel?.ShutdownAsync().Wait();
            _channel = null;

            isConnected = false;
            Debug.Log("Client cleaned !");
            return true;
        }
        
        #region Handshake
        
        async Task<bool> Handshake()
        {
            Debug.Log("GRPCClient.cs > Handshake...");
            var response = await client.GRPC_HandshakeAsync(new GRPC_HandshakePost());
            Debug.Log($"GRPCClient.cs > Handshake result: {response.Result}");

            if (response.Result == 0)
            {
                FU_NetworkObjectManager.instance.ComputeNetObjUpdates(response.NetObjects.ToList());
            }
            
            return response.Result == 0;
        }
        #endregion
    }
}