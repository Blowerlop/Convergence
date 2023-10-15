using System;
using System.IO;
using System.Reflection.Emit;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
using Type = System.Type;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>  where T : struct
    {
        private MainService.MainServiceClient _client => GRPC_Transport.instance.client;
        
        private static AsyncClientStreamingCall<GRPC_NetVarUpdate, GRPC_EmptyMsg> _sendStream;
        private static CancellationTokenSource _sendStreamCancellationTokenSource;

        private int _variableHashName;
        private static GRPC_GenericType _genericType;
        
        
        public GRPC_NetworkVariable(string nameOfVariable, T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
            _variableHashName = nameOfVariable.GetHashCode();
            if (_sendStream == null)
            {
                _genericType = GetGrpcGenericType();
                if (_genericType == GRPC_GenericType.Null) return;
                
                _sendStream = _client.GRPC_SrvNetVarUpdate();
                _sendStreamCancellationTokenSource = new CancellationTokenSource();

                
                GRPC_NetworkManager.instance.onClientEndEvent.Subscribe(this, Dispose);
            }
            
            OnValueChanged += OnValueChange;
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
                await _sendStream.RequestStream.WriteAsync(new GRPC_NetVarUpdate() {HashName = _variableHashName, NewValue = new GRPC_GenericValue() {Type = _genericType, Value = value}}, _sendStreamCancellationTokenSource.Token);
                Debug.Log($"Network Variable updated, send info to the grpcServer... Value : {value}");
            }
            catch (IOException)
            {
                GRPC_NetworkManager.instance.StopClient();
            }
        }

        private GRPC_GenericType GetGrpcGenericType()
        {
            Type type = typeof(T);

            if (type == typeof(Int32))
            {
                return GRPC_GenericType.Int;
            }
            else if (type == typeof(string))
            {
                return GRPC_GenericType.String;
            }
            else if (type == typeof(bool))
            {
                return GRPC_GenericType.Bool;
            }
            else if (type == typeof(Vector3))
            {
                return GRPC_GenericType.Vector3;
            }
            else
            {
                Debug.LogError($"The type '{type}' is not supported");
                return GRPC_GenericType.Null;
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
            
            GRPC_NetworkManager.instance.onClientEndEvent.Unsubscribe(Dispose);
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
