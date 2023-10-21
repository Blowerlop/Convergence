using System;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using BestHTTP.JSON;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GRPCClient;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
using Type = System.Type;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>  where T : struct
    {
        private static MainService.MainServiceClient _client => GRPC_Transport.instance.client;
        
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
                if (_genericType == GRPC_GenericType.Isnull) return;
                
                _sendStream = _client.GRPC_SrvNetVarUpdate();
                _sendStreamCancellationTokenSource = new CancellationTokenSource();

                
                GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, Dispose);
            }
            
            OnValueChanged += OnValueChange;
        }
        
        
        private void OnValueChange(T _, T newValue)
        {
            UpdateVariableOnGrpc(newValue);
        }

        private async void UpdateVariableOnGrpc(T newValue)
        {
            // if (CanClientWrite(GetBehaviour().OwnerClientId) == false) return;
            
            try
            {
                string jsonEncode = JsonConvert.SerializeObject(newValue);
                int netId = (int)GetBehaviour().GetComponentInParent<NetworkObject>().NetworkObjectId;
                await _sendStream.RequestStream.WriteAsync(new GRPC_NetVarUpdate() {NetId = netId, HashName = _variableHashName, NewValue = new GRPC_GenericValue() {Type = _genericType, Value = jsonEncode}}, _sendStreamCancellationTokenSource.Token);
                Debug.Log($"Network Variable updated, send info to the grpcServer... Value : {newValue}");
            }
            catch (IOException)
            {
                GRPC_NetworkManager.instance.StopClient();
            }
        }

        private static GRPC_GenericType GetGrpcGenericType()
        {
            Type type = typeof(T);

            if (type == typeof(Int32))
            {
                return GRPC_GenericType.Int;
            }

            if (type == typeof(string))
            {
                return GRPC_GenericType.String;
            }

            if (type == typeof(bool))
            {
                return GRPC_GenericType.Bool;
            }

            if (type == typeof(Vector3))
            {
                return GRPC_GenericType.Vector3;
            }

            Debug.LogError($"The type '{type}' is not supported");
            return GRPC_GenericType.Isnull;
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _sendStreamCancellationTokenSource?.Cancel();
            _sendStreamCancellationTokenSource?.Dispose();
            _sendStreamCancellationTokenSource = null;
            
            _sendStream?.Dispose();
            _sendStream = null;
            
            GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(Dispose);
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
