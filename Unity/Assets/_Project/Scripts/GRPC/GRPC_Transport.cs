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
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    [RequireComponent(typeof(GRPC_NetworkManager))]
    [DisallowMultipleComponent]
    public class GRPC_Transport : MonoBehaviour
    {
        [SerializeField] private string _address = "127.0.0.1";
        [SerializeField] private ushort _port = 5001;
        public bool isConnected { get; private set; }
        private GrpcChannel _channel;
        private MainService.MainServiceClient _client;
        public int rtt;
        
        //Ping
        private readonly CancellationTokenSource _pingCancelSrc = new();
        private AsyncDuplexStreamingCall<GRPC_PingPost, GRPC_PingGet> _pingStream;
        
        private readonly Stopwatch _pingSW = new();
        
        
        private void Start()
        {
            //Need this to not see all Best HTTP Debug.LogError
            //Best HTTP logs even if exceptions are caught
            HTTPManager.Logger.Level = Loglevels.None;
        }


        public async void StartClient()
        {
            if (isConnected)
            {
                Debug.LogError("Can't start a client when there is one already running");
                return;
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

            _client = new MainService.MainServiceClient(_channel);
            
            var result = await NHandshake();
            
            if(result)
                StartPinging();
        }

        public async void StopClient()
        {
            if (!isConnected)
            {
                Debug.LogError("No client are running");
                return;
            }
            
            Debug.Log("Connection shutdown ! Cleaning client...");
            
            _pingCancelSrc?.Cancel();
            
            _pingStream?.Dispose();
            _pingStream = null;
            
            _channel?.ShutdownAsync().Wait();
            _channel = null;
            
            Debug.Log("Client cleaned !");
        }
        
        #region Handshake
        
        async Task<bool> NHandshake()
        {
            Debug.Log("GRPCClient.cs > NHandshake...");
            var response = await _client.GRPC_NetcodeHandshakeAsync(new GRPC_NHandshakePost());
            Debug.Log($"GRPCClient.cs > NHandshake result: {response.Result}");
            
            return response.Result == 0;
        }
        #endregion
        
        #region Ping
        
        //Timer => really not great, here for testing purpose only
        //Need better tick system
        private float _timer = 0;
        private void Update()
        {
            if (_pingStream == null) return;
            
            if(_timer < 1f/3f)
                _timer += Time.deltaTime;
            else
            {
                _timer = 0;
                PingPost();
            }
        }
        
        private void StartPinging()
        {
            _pingStream = _client.GRPC_Ping();
            PingGet();
        }
        
        //Called when we want to ping the server
        private async void PingPost()
        {
            _pingSW.Restart();

            try
            {
                await _pingStream.RequestStream.WriteAsync(new GRPC_PingPost(), _pingCancelSrc.Token);
            }
            catch (IOException)
            {
                StopClient();
            }
        }
        
        //Server got our ping, we get a ping back. We can get the ping time here
        private async void PingGet()
        {
            try
            {
                while (await _pingStream.ResponseStream.MoveNext(_pingCancelSrc.Token))
                {
                    _pingSW.Stop();
                    Debug.Log("Your ping is: " + _pingSW.ElapsedMilliseconds + "ms");
                }
            }
            catch (RpcException)
            {
                StopClient();
            }
        }
        
        #endregion
    }
}