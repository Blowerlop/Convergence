using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [System.Serializable]
    public class FU_NetworkVariableReadOnly : IDisposable
    {
        [SerializeField] private int _value;
        private MainService.MainServiceClient _client => FU_GRPC_Transport.instance.client;
        
        private AsyncServerStreamingCall<GRPC_NetVarUpdate> _readStream;
        private CancellationTokenSource _readStreamCancellationTokenSource;
        
        
        public FU_NetworkVariableReadOnly()
        {
            // _readStream = _client.GRPC_CliNetNetVarUpdate(new GRPC_NetVarUpdate() {HashName = "test".GetHashCode()});
            _readStreamCancellationTokenSource = new CancellationTokenSource();
            ReadValues();
        }
        
        ~FU_NetworkVariableReadOnly()
        {
            Dispose();
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
                    // _value = Convert(_readStream.ResponseStream.Current.NewValue);
                    _value = int.Parse(_readStream.ResponseStream.Current.NewValue.Value);
                    Debug.Log($"Network variable sync : {_value.ToString()}");
                }
            }
            catch
            {
                FU_GRPC_NetworkManager.instance.StopClient();
            }
        }
        
        /// TO PLACE SOMEWHERE ELSE
        private dynamic Convert(GRPC_GenericValue grpcGenericValue)
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
                    // return (Vector3)grpcGenericValue.Value
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return null;
        }
    }
}
