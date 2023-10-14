using System;
using System.IO;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>  where T : struct
    {
        private MainService.MainServiceClient _client => GRPC_Transport.instance.client;
        
        private AsyncClientStreamingCall<GRPC_NetVarUpdate, GRPC_EmptyMsg> _sendStream;
        private CancellationTokenSource  _sendStreamCancellationTokenSource = new CancellationTokenSource();

        private int _variableHashName;
        
        
        public GRPC_NetworkVariable(string nameOfVariable, T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
            // #if UNITY_EDITOR
            // if (GetBehaviour() == null) return;
            // #endif
            // if (CanClientWrite(GetBehaviour().OwnerClientId) == false) return;

            _variableHashName = nameOfVariable.GetHashCode();
            _sendStream = _client.GRPC_SrvNetVarUpdate();
            OnValueChanged += OnValueChange;
        }

        ~GRPC_NetworkVariable()
        {
            if (CanClientWrite(GetBehaviour().OwnerClientId) == false) return;

            Dispose();
        }

        
        private void OnValueChange(T _, T newValue)
        {
            UpdateVariableOnGrpc();
        }

        private async void UpdateVariableOnGrpc()
        {
            try
            {
                string value = Random.Range(1, 5).ToString();
                await _sendStream.RequestStream.WriteAsync(new GRPC_NetVarUpdate() {HashName = _variableHashName, NewValue = new GRPC_GenericValue() {Type = GRPC_GenericType.Int, Value = value}}, _sendStreamCancellationTokenSource.Token);
                Debug.Log($"Network Variable updated, send info to the grpcServer... Value : {value}");
            }
            catch (IOException)
            {
                GRPC_NetworkManager.instance.StopClient();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _sendStreamCancellationTokenSource?.Cancel();
            _sendStreamCancellationTokenSource?.Dispose();
            
            _sendStream?.Dispose();
        }
    }
}
