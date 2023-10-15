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
        
        private static AsyncClientStreamingCall<GRPC_NetVarUpdate, GRPC_EmptyMsg> _sendStream;
        private static CancellationTokenSource _sendStreamCancellationTokenSource;

        private int _variableHashName;
        
        
        public GRPC_NetworkVariable(string nameOfVariable, T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
            // #if UNITY_EDITOR
            // if (GetBehaviour() == null) return;
            // #endif
            // 

            _variableHashName = nameOfVariable.GetHashCode();
            if (_sendStream == null)
            {
                _sendStream = _client.GRPC_SrvNetVarUpdate();
                _sendStreamCancellationTokenSource = new CancellationTokenSource();
                GRPC_NetworkManager.instance.onClientEndedEvent.Subscribe(this, Dispose);
            }
            
            OnValueChanged += OnValueChange;
        }
        
        private void OnValueChange(T _, T newValue)
        {
            UpdateVariableOnGrpc();
        }

        private async void UpdateVariableOnGrpc()
        {
            // if (CanClientWrite(GetBehaviour().OwnerClientId) == false) return;
            
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
            _sendStreamCancellationTokenSource = null;
            
            _sendStream?.Dispose();
            _sendStream = null;
            
            GRPC_NetworkManager.instance.onClientEndedEvent.Unsubscribe(Dispose);
        }
        
        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStaticVariables()
        {
            _sendStream = null;
            _sendStreamCancellationTokenSource = null;
        }
        #endif
    }
}
