using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{
    public class GRPC_Rtt
    {
        // private 
        //
        // private void StartPinging()
        // {
        //     _pingStream = _client.GRPC_Ping();
        //     PingGet();
        // }
        //
        // //Called when we want to ping the server
        // private async void PingPost()
        // {
        //     _pingSW.Restart();
        //
        //     try
        //     {
        //         await _pingStream.RequestStream.WriteAsync(new GRPC_PingPost(), _pingCancelSrc.Token);
        //     }
        //     catch (IOException)
        //     {
        //         StopClient();
        //     }
        // }
        //
        // //Server got our ping, we get a ping back. We can get the ping time here
        // private async void PingGet()
        // {
        //     try
        //     {
        //         while (await _pingStream.ResponseStream.MoveNext(_pingCancelSrc.Token))
        //         {
        //             _pingSW.Stop();
        //             Debug.Log("Your ping is: " + _pingSW.ElapsedMilliseconds + "ms");
        //         }
        //     }
        //     catch (RpcException)
        //     {
        //         StopClient();
        //     }
        // }
    }
}
