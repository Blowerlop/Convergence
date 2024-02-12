using System;
using GRPCClient;
using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using UnityEngine;

namespace Project
{
    [Serializable]
    public class FU_NetworkVariableReadOnly<T> : IDisposable
    {
        private FU_GRPC_NetworkManager NetworkManager => FU_GRPC_NetworkManager.instance;
        private FU_NetVarHandler NetVarHandler => FU_NetVarHandler.instance;

        private readonly int _variableHashName;
        [ShowInInspector, Sirenix.OdinInspector.ReadOnly] public T value { get; private set; }
        private int _netId;

        private static GRPC_GenericType _currentType = GRPC_GenericType.Isnull;
        
        public event Action<T> OnValueChanged;
        
        public FU_NetworkVariableReadOnly(string variableName)
        {
            _variableHashName = variableName.ToLower().ToHashIsSameAlgoOnUnreal();
        }

        public void Dispose()
        {
            if(FU_GRPC_NetworkManager.IsInstanceAlive() && NetworkManager != null) 
                NetworkManager.onClientStopEvent.Unsubscribe(Dispose);
            
            if(FU_NetVarHandler.IsInstanceAlive() && NetVarHandler != null) 
                NetVarHandler.GetEvent(_currentType).Unsubscribe(OnNetVarUpdated);
        }

        public void Initialize(FU_NetworkObject obj)
        {
            _netId = (int)obj.NetID;
            
            if (NetworkManager.isConnected)
            {
                GRPC_NetworkVariable_Initialization();
            }
            else
            {
                NetworkManager.onClientStartedEvent.Subscribe(this, GRPC_NetworkVariable_Initialization);
            }
        }

        private void GRPC_NetworkVariable_Initialization()
        {
            NetworkManager.onClientStartedEvent.Unsubscribe(GRPC_NetworkVariable_Initialization);
            
            if(_currentType == GRPC_GenericType.Isnull) _currentType = GetGrpcGenericType();
            
            NetworkManager.onClientStopEvent.Subscribe(this, Dispose);
            
            NetVarHandler.GetEvent(_currentType).Subscribe(this, OnNetVarUpdated);
            NetVarHandler.TryCreateStream(_currentType);
        }

        private void OnNetVarUpdated(GRPC_NetVarUpdate update)
        {
            if (update.HashName != _variableHashName || update.NetId != _netId) return;
            
            object dynamicValueObject = ConvertGrpcGenericType(update.NewValue);
            value = (T)dynamicValueObject;
                        
            OnValueChanged?.Invoke(value);
                        
            Debug.Log($"Network variable sync : {value.ToString()}");
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

            if (type == typeof(FixedString32Bytes) || type == typeof(FixedString64Bytes) || type == typeof(FixedString128Bytes) || type == typeof(string))
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
        
        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStaticFields()
        {
            _currentType = GRPC_GenericType.Isnull;
        }
        #endif
    }
}
