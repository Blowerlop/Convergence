using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using GRPC.NET;
using Grpc.Net.Client;
using TestServer;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Project
{
    public class HelloExample : MonoBehaviour
    {
        private TestService.TestServiceClient client;
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
        
            client = new TestService.TestServiceClient(_channel);

            await SayHello();
        }

        async Task SayHello()
        {
            Debug.Log("HelloExample.cs > Sending hello...");
            var response = await client.SayHelloAsync(new HelloRequest { Msg = "Hello !"});
            Debug.Log($"HelloExample.cs > Response: {response.Msg}");
        }
        
        public async void Ping()
        {
            Stopwatch sw = new Stopwatch();
        
            sw.Start();
        
            await client.PingAsync(new PingPost { Time = Random.Range(-9999, 9999) });

            sw.Stop();
        
            Debug.Log("Your ping is: " + sw.ElapsedMilliseconds);
        }
    }
}
