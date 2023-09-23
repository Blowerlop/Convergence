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
    public class FakeUnrealClient : MonoBehaviour
    {
        //GRPC
        private MainService.MainServiceClient _client;
        private GrpcChannel _channel;
        
        public bool IsAlive => _channel != null;
        
        //Ping
        private readonly CancellationTokenSource _pingCancelSrc = new();
        private AsyncDuplexStreamingCall<PingPost, PingGet> _pingStream;
        
        private readonly Stopwatch _pingSW = new();
        
        private async void Start()
        {
            //Need this to not see all Best HTTP Debug.LogError
            //Best HTTP logs even if exceptions are caught
            HTTPManager.Logger.Level = Loglevels.None;
            
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif
            
            HTTPManager.Setup();
            GRPCBestHttpHandler httpHandler = new GRPCBestHttpHandler();
            
            _channel  = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });
        
            _client = new MainService.MainServiceClient(_channel);

            var result = await Handshake();
            
            if(result)
                StartPinging();
        }

        private void CleanClient()
        {
            if (!IsAlive) return;
            
            Debug.Log("Connection shutdown. Clean client.");
            
            _pingCancelSrc?.Cancel();
            
            _pingStream?.Dispose();
            _pingStream = null;
            
            _channel?.ShutdownAsync().Wait();
            _channel = null;
        }
        
        private void OnDestroy()
        {
            CleanClient();
        }

        #region Handshake
        
        async Task<bool> Handshake()
        {
            Debug.Log("FakeUnrealClient.cs > Handshake...");
            var response = await _client.HandshakeAsync(new HandshakePost());
            Debug.Log($"FakeUnrealClient.cs > Handshake result: {response.Result}");
            
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
            _pingStream = _client.Ping();
            PingGet();
        }
        
        //Called when we want to ping the server
        private async void PingPost()
        {
            _pingSW.Restart();

            try
            {
                await _pingStream.RequestStream.WriteAsync(new PingPost(), _pingCancelSrc.Token);
            }
            catch (IOException)
            {
                CleanClient();
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
                CleanClient();
            }
        }
        
        #endregion
    }
}