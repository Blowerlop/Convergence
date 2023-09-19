using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GRPC.NET;
using Grpc.Net.Client;
using GRPCClient;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    public class HelloExample : MonoBehaviour
    {
        private MainService.MainServiceClient client;
        private GrpcChannel _channel;
        
        private async void Start()
        {
            Application.runInBackground = true;
        
            BestHTTP.HTTPManager.Setup();
            GRPCBestHttpHandler httpHandler = new GRPCBestHttpHandler();
            
            _channel  = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });
        
            client = new MainService.MainServiceClient(_channel);

            await NHandshake();
            Pong();
        }

        async Task NHandshake()
        {
            Debug.Log("HelloExample.cs > Sending hello...");
            var response = await client.NetcodeHandshakeAsync(new NHandshakePost());
            Debug.Log($"HelloExample.cs > Handshake result: {response.Result}");
        }

        private float timer = 0;
        private void Update()
        {
            if (pingStream == null) return;
            
            if(timer < 1f/3f)
                timer += Time.deltaTime;
            else
            {
                timer = 0;
                Ping();
            }
        }
        
        private AsyncDuplexStreamingCall<PingPost, PingGet> pingStream;
        Stopwatch pingSW = new Stopwatch();
        private async void Ping()
        {
            pingSW.Restart();
            await pingStream.RequestStream.WriteAsync(new PingPost());
        }
        
        private async void Pong()
        {
            pingStream = client.Ping(new Metadata { {"client_id", "0" }});

            try
            {
                while (await pingStream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    pingSW.Stop();
                    Debug.Log("Your ping is: " + pingSW.ElapsedMilliseconds + "ms");
                }
            }
            catch (IOException)
            {
                Debug.Log("BRUUUUUUUUUUUUH");
            }
        }
    }
}
