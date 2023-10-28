using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Newtonsoft.Json;
using Project.Extensions;
using Unity.Netcode;
using UnityEngine;
using Type = System.Type;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>  where T : struct
    {
        private static MainService.MainServiceClient _client => GRPC_Transport.instance.client;
        
        private static AsyncClientStreamingCall<GRPC_NetVarUpdate, GRPC_EmptyMsg> _sendStream;
        private static CancellationTokenSource _sendStreamCancellationTokenSource;

        private readonly int _variableHashName;
        private static GRPC_GenericType _currentType = GRPC_GenericType.Isnull;
        
        
        public GRPC_NetworkVariable(string variableName, T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
            _variableHashName = variableName.ToLower().ToHashIsSameAlgoOnReal();
            if (_sendStream == null)
            {
                _currentType = GetGrpcGenericType();
                
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
            try
            {
                object valueToEncodeInJson;
                bool autoJsonSerialization = true;
                
                if (newValue is NetworkString)
                {
                    NetworkString networkString = (NetworkString)Convert.ChangeType(newValue, typeof(NetworkString));
                    valueToEncodeInJson = networkString.value;
                }
                else if (newValue is NetworkVector3Simplified)
                {
                    autoJsonSerialization = false;
                    NetworkVector3Simplified networkString = (NetworkVector3Simplified)Convert.ChangeType(newValue, typeof(NetworkVector3Simplified));
                    valueToEncodeInJson = $"X={networkString.x},Y={networkString.y},Z={networkString.z}";
                }
                else
                {
                    valueToEncodeInJson = newValue;
                }
                
                string jsonEncode;
                if (autoJsonSerialization)
                {
                    jsonEncode = JsonConvert.SerializeObject(valueToEncodeInJson, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
                else
                {
                    jsonEncode = (string)valueToEncodeInJson;
                }
                
                int netId = (int)GetBehaviour().GetComponentInParent<NetworkObject>().NetworkObjectId;
                GRPC_NetVarUpdate result = new GRPC_NetVarUpdate()
                {
                    NetId = netId, HashName = _variableHashName, NewValue = new GRPC_GenericValue {Type = _currentType, Value = jsonEncode }
                };
                await _sendStream.RequestStream.WriteAsync(result, _sendStreamCancellationTokenSource.Token);
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

            if (type == typeof(NetworkString))
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
            
            if (type == typeof(NetworkVector3Simplified))
            {
                return GRPC_GenericType.Vector3;
            }
            
            if (type == typeof(Quaternion))
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
            _currentType = GRPC_GenericType.Isnull;
            _sendStream = null;
            _sendStreamCancellationTokenSource = null;
        }
        #endif
    }
}
