using System.Threading;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class TeamSelection : MonoBehaviour
    {
        private AsyncDuplexStreamingCall<GRPC_TeamResponse, GRPC_Team> _teamManagerStream;
        private CancellationTokenSource _cancellationTokenSource;

        private void OnEnable()
        {
            GRPC_NetworkManager.instance.onClientStartedEvent += InitGrpcStream;
            GRPC_NetworkManager.instance.onClientStopEvent += DisposeGrpcStream;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer == false) return;
            
            if (GRPC_NetworkManager.IsInstanceAlive())
            {
                GRPC_NetworkManager.instance.onClientStartedEvent -= InitGrpcStream;
                GRPC_NetworkManager.instance.onClientStopEvent -= DisposeGrpcStream;
                DisposeGrpcStream();
            }
        }

        private void InitGrpcStream()
        {
            if (NetworkManager.Singleton.IsServer == false) return;
            
            _teamManagerStream = GRPC_NetworkManager.instance.client.GRPC_TeamSelectionGrpcToNetcode();
            _cancellationTokenSource = new CancellationTokenSource();
            
            Read();
        }

        private void DisposeGrpcStream()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer == false) return;
            
            // _cancellationTokenSource?.Cancel();
            // _cancellationTokenSource?.Dispose();
            // _cancellationTokenSource = null;
            
            // _teamManagerStream?.Dispose();
            // _teamManagerStream = null;
        }

        private void Write(GRPC_TeamResponse response)
        {
            GRPC_NetworkLoop.instance.AddMessage(new GRPC_Message<GRPC_TeamResponse>(_teamManagerStream.RequestStream, response, _cancellationTokenSource));
        }

        private async void Read()
        {
            try
            {
                while (await _teamManagerStream.ResponseStream.MoveNext(_cancellationTokenSource.Token))
                {
                    Debug.Log("Team message received");
                    GRPC_Team messageReceived = _teamManagerStream.ResponseStream.Current;
                    bool response = TeamManager.instance.TrySetTeam(messageReceived.ClientId, messageReceived.TeamIndex,
                        PlayerPlatform.Mobile);
                    Write(new GRPC_TeamResponse { Team = messageReceived, Response = response });
                }
            }
            catch (RpcException e)
            {
                Debug.LogError(e);
            }
        }
    }
}