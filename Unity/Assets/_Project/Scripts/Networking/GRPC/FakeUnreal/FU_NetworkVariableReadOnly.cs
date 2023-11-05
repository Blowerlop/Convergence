using System;
using System.Globalization;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [Serializable]
    public class FU_NetworkVariableReadOnly<T> : IDisposable
    {
        private MainService.MainServiceClient _client => FU_GRPC_Transport.instance.client;

        private readonly int _variableHashName;
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public T value { get; private set; }
        private int _netId;
        
        private static AsyncServerStreamingCall<GRPC_NetVarUpdate> _readStream;
        private static CancellationTokenSource _readStreamCancellationTokenSource;
        [ClearOnReload(GRPC_GenericType.Isnull)] private static GRPC_GenericType _currentType = GRPC_GenericType.Isnull;
        
        
        
        public FU_NetworkVariableReadOnly(string variableName)
        {
            _variableHashName = variableName.ToLower().ToHashIsSameAlgoOnUnreal();
        }
        
        ~FU_NetworkVariableReadOnly()
        {
            Dispose();
        }

        public void Initialize(NetworkBehaviour networkBehaviour)
        {
            _netId = (int)networkBehaviour.NetworkObjectId;
            
            if (FU_GRPC_NetworkManager.instance.isConnected)
            {
                GRPC_NetworkVariable_Initialization();
            }
            else
            {
                FU_GRPC_NetworkManager.instance.onClientStartedEvent.Subscribe(this, GRPC_NetworkVariable_Initialization);
            }
        }

        private void GRPC_NetworkVariable_Initialization()
        {
            if (_readStream == null)
            {
                _readStream = _client.GRPC_CliNetNetVarUpdate(new GRPC_GenericValue {Type = GetGrpcGenericType()});
                _readStreamCancellationTokenSource = new CancellationTokenSource();

                GRPC_NetworkManager.instance.onClientStopEvent.Subscribe(this, OnClientStop);
                _currentType = GetGrpcGenericType();
            }
            
            ReadValues();
        }
        
        
        public void Dispose()
        {
            _readStreamCancellationTokenSource?.Cancel();
            _readStreamCancellationTokenSource?.Dispose();
            
            _readStream?.Dispose();
        }



        private async void ReadValues()
        {
            try
            {
                while (await _readStream.ResponseStream.MoveNext(_readStreamCancellationTokenSource.Token))
                {
                    if (_readStream.ResponseStream.Current.HashName == _variableHashName &&
                        _readStream.ResponseStream.Current.NetId == _netId)
                    {
                        // value = ConvertGrpcGenericType(_readStream.ResponseStream.Current.NewValue);
                        object dynamicValueObject = ConvertGrpcGenericType(_readStream.ResponseStream.Current.NewValue);
                        value = (T)dynamicValueObject;
                        
                        Debug.Log($"Network variable sync : {value.ToString()}");
                    }
                    
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        /// TO PLACE SOMEWHERE ELSE
        private dynamic ConvertGrpcGenericType(GRPC_GenericValue grpcGenericValue)
        {
            switch (grpcGenericValue.Type)
            {
                case GRPC_GenericType.Int:
                    return int.Parse(grpcGenericValue.Value);
                case GRPC_GenericType.String:
                    return grpcGenericValue.Value;
                case GRPC_GenericType.Bool:
                    return bool.Parse(grpcGenericValue.Value);
                case GRPC_GenericType.Vector3:
                    string val = grpcGenericValue.Value.Replace("X=", "");
                    val = val.Replace("Y=", "");
                    val = val.Replace("Z=", "");
                    string[] splitValue = val.Split(',');
                    return new Vector3(float.Parse(splitValue[0]), float.Parse(splitValue[1]), float.Parse(splitValue[2]));
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static GRPC_GenericType GetGrpcGenericType()
        {
            Type type = typeof(T);

            if (type == typeof(Int32))
            {
                return GRPC_GenericType.Int;
            }

            if (type == typeof(FixedString32Bytes) || type == typeof(FixedString64Bytes) || type == typeof(FixedString128Bytes))
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
        
        private void OnClientStop()
        {
            _readStreamCancellationTokenSource?.Cancel();
            _readStreamCancellationTokenSource?.Dispose();
            _readStreamCancellationTokenSource = null;
            
            _readStream?.Dispose();
            _readStream = null;

            if (FU_GRPC_NetworkManager.isBeingDestroyed == false)
            {
                FU_GRPC_NetworkManager.instance.onClientStartedEvent.Unsubscribe(GRPC_NetworkVariable_Initialization);
                FU_GRPC_NetworkManager.instance.onClientStopEvent.Unsubscribe(OnClientStop);
            }
        }
        
        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStaticFields()
        {
            _readStream = null;
            _readStreamCancellationTokenSource = null;
            _currentType = GRPC_GenericType.Isnull;
        }
        #endif
    }
}
