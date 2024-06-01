using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GRPCClient;
using Newtonsoft.Json;
using Project.Extensions;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Type = System.Type;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>  where T : struct
    {
        private MainService.MainServiceClient _client => GRPC_Transport.instance.client;
        
        private AsyncClientStreamingCall<GRPC_NetVarUpdate, GRPC_EmptyMsg> _sendStream;
        private CancellationTokenSource _sendStreamCancellationTokenSource;

        
        [ShowInInspector] private readonly int _variableHashName;
        private GRPC_GenericType _currentType = GRPC_GenericType.Isnull;
        private int _netId;
        private NetworkBehaviour _networkBehaviour;

        private bool _isGrpcSync;
        private GRPC_NetworkObjectSyncer _netObjectSyncer;

        
        public GRPC_NetworkVariable(string variableName, T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
            _variableHashName = variableName.ToLower().ToHashIsSameAlgoOnUnreal();
        }

        // NOT WORKING ! WHY ?
        // ~GRPC_NetworkVariable()
        // {
        //     //Reset();
        //     OnClientStop();
        // }


        public void Initialize()
        {
            _networkBehaviour = GetBehaviour();
            if (!_networkBehaviour.IsServer && !_networkBehaviour.IsHost) return;
            
            if (GRPC_NetworkManager.instance.isConnected)
            {
                GRPC_NetworkVariable_Initialization();
            }

            GRPC_NetworkManager.instance.onClientStartedEvent += GRPC_NetworkVariable_Initialization;
            GRPC_NetworkManager.instance.onClientStopEvent += OnClientStop;
        }

        public void Reset()
        {
            if (!GRPC_NetworkManager.instance.isConnected) return;
            if (!_networkBehaviour.IsServer && !_networkBehaviour.IsHost) return;
            
            if (GRPC_NetworkManager.IsInstanceAlive() == false) return;
            
            GRPC_NetworkManager.instance.onClientStartedEvent -= GRPC_NetworkVariable_Initialization;
            //GRPC_NetworkManager.instance.onClientStopEvent -= OnClientStop;

            OnClientStop();
        }

        

        private void GRPC_NetworkVariable_Initialization()
        {
             NetworkBehaviour networkBehaviour = GetBehaviour();

            // Server authoritative only 
            // Only the server open NetworkVariable streams
            if (!networkBehaviour.IsServer && !networkBehaviour.IsHost) return;
            _netId = (int)networkBehaviour.GetComponentInParent<NetworkObject>().NetworkObjectId;
            
            _currentType = GetGrpcGenericType();
            _netObjectSyncer = networkBehaviour.GetComponentInParent<GRPC_NetworkObjectSyncer>();
            _netObjectSyncer.onNetworkObjectHasSpawnedOnGrpc += Sync;
            
            _sendStream = _client.GRPC_SrvNetVarUpdate();
            _sendStreamCancellationTokenSource = new CancellationTokenSource();
            
            
            Sync();
            GetBehaviour().StartCoroutine(WaitAndTrySyncNewUnrealClient());
            
            OnValueChanged += OnValueChange_WriteInStream;

            _isGrpcSync = true;
        }

        private IEnumerator WaitAndTrySyncNewUnrealClient()
        {
            yield return new WaitForSeconds(5.0f);
            
            GRPC_NetworkObjectSyncer syncer = GetBehaviour().GetComponentInParent<GRPC_NetworkObjectSyncer>();
            if (syncer == null)
            {
                Debug.LogError("NetworkObject has not NetworkObjectSyncer component");
                yield break;
            }

            if (!syncer.IsOwnedByUnrealClient) yield break;
            
            if (syncer.IsOwnedByUnrealClient)
            {
                // Debug.Log("Unreal client connected, sync vars");
                // Sync();
            }
        }

        private void OnValueChange_WriteInStream(T _, T newValue)
        {
            UpdateVariableOnGrpc(newValue);
        }

        private void UpdateVariableOnGrpc(T newValue)
        {
            if (GRPC_NetworkManager.instance.isConnected == false) return;

            if (_netObjectSyncer.hasBeenSpawnedOnGrpc == false) return;
            
            try
            {
                string jsonEncode = ValueToJson(newValue);
                
                GRPC_NetVarUpdate result = new GRPC_NetVarUpdate()
                {
                    NetId = _netId, HashName = _variableHashName, NewValue = new GRPC_GenericValue {Type = _currentType, Value = jsonEncode }
                };

                if (_sendStream is { RequestStream: not null } 
                    && _sendStreamCancellationTokenSource is { IsCancellationRequested: false })
                {
                    GRPC_NetworkLoop.instance.AddMessage(new GRPC_Message<GRPC_NetVarUpdate>(_sendStream.RequestStream, result, _sendStreamCancellationTokenSource));
                }
            }
            catch (IOException e)
            {
                Debug.LogError(e);
            }
        }

        public void Sync()
        {
            UpdateVariableOnGrpc(Value);
        }

        private string ValueToJson(T newValue)
        {
            object valueToEncodeInJson;
            bool autoJsonSerialization = true;
                
            if (newValue is FixedString32Bytes or FixedString64Bytes or FixedString128Bytes)
            {
                if (newValue is FixedString32Bytes string32Bytes)
                {
                    valueToEncodeInJson = string32Bytes.Value;
                }
                else if (newValue is FixedString64Bytes string64Bytes)
                {
                    valueToEncodeInJson = string64Bytes.Value;
                }
                else if (newValue is FixedString128Bytes string128Bytes)
                {
                    valueToEncodeInJson = string128Bytes.Value;
                }
                else
                {
                    valueToEncodeInJson = null;
                }
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

            return jsonEncode;
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

        //Can't use dispose because it is called on
        //NetworkVariables by Netcode when a NetworkObject is despawned
        private async void OnClientStop()
        {
            if (_isGrpcSync)
            {
                OnValueChanged -= OnValueChange_WriteInStream;
            }

            _netObjectSyncer.onNetworkObjectHasSpawnedOnGrpc -= Sync;
            
            Debug.Log($"Start dispose : Hash {_variableHashName} / {_networkBehaviour.GetType()} / {_networkBehaviour.gameObject.name} / Net id {_netId}", _networkBehaviour);
            
            await _sendStream.RequestStream.CompleteAsync();
            await Task.Delay(5);
            
            _sendStreamCancellationTokenSource.Cancel();
            
            // try
            // {
            //     Debug.Log($"Net var disposed : status {_sendStream.GetStatus()} 3");
            // }
            // catch
            // {
            //     Debug.Log($"Net var disposed : status not complete 33");
            // }
            
            _sendStreamCancellationTokenSource.Dispose();
            // try
            // {
            //     Debug.Log($"Net var disposed : status {_sendStream.GetStatus()} 4");
            // }
            // catch
            // {
            //     Debug.Log($"Net var disposed : status not complete 4");
            // }
            
            _sendStreamCancellationTokenSource = null;
            // try
            // {
            //     Debug.Log($"Net var disposed : status {_sendStream.GetStatus()} 5");
            // }
            // catch
            // {
            //     Debug.Log($"Net var disposed : status not complete 5");
            // }

            _sendStream.Dispose();
            // try
            // {
            //     Debug.Log($"Net var disposed : status {_sendStream.GetStatus()} 6");
            // }
            // catch
            // {
            //     Debug.Log($"Net var disposed : status not complete 6");
            // }
            
            _sendStream = null;
            // try
            // {
            //     Debug.Log($"Net var disposed : status {_sendStream.GetStatus()} 7");
            // }
            // catch
            // {
            //     Debug.Log($"Net var disposed : status not rthnrthrnth 7");
            // }

            _isGrpcSync = false;
            
            Debug.Log($"End dispose : Hash {_variableHashName} / Net id {_netId}");
        }
    }
}
