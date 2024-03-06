using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public abstract class GRPC_Message
    {
        public abstract Task WriteAsync();
        public abstract Type GetMessageType();
    }
    
    public class GRPC_Message<T> : GRPC_Message
    {
        private readonly IClientStreamWriter<T> _streamWriter;
        private readonly T _message;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        
        public GRPC_Message(IClientStreamWriter<T> streamWriter, T message, CancellationTokenSource cancellationTokenSource)
        {
            _streamWriter = streamWriter;
            _message = message;
            _cancellationTokenSource = cancellationTokenSource;
        }
        
        public override Task WriteAsync()
        {
            return _streamWriter.WriteAsync(_message, _cancellationTokenSource.Token);
        }

        public override Type GetMessageType()
        {
            return _message.GetType();
        }
    }
    
    public class GRPC_NetworkLoop : NetworkSingleton<GRPC_NetworkLoop>, INetworkUpdateSystem 
    {
        private readonly Stack<GRPC_Message> _messages = new Stack<GRPC_Message>();
        private int _currentTick;

        
        public override void OnNetworkSpawn()
        {
            this.RegisterNetworkUpdate(NetworkUpdateStage.PostLateUpdate);
        }

        public override void OnNetworkDespawn()
        {
            this.UnregisterNetworkUpdate(NetworkUpdateStage.PostLateUpdate);
        }

        
        public void AddMessage(GRPC_Message message)
        {
            _messages.Push(message);
        }

        public async void NetworkUpdate(NetworkUpdateStage updateStage)
        {
            if (updateStage == NetworkUpdateStage.PostLateUpdate)
            {
                _currentTick++;
                int currentTick = _currentTick;
                
                while (_messages.Any() || currentTick == _currentTick)
                {
                    GRPC_Message message = _messages.Pop();
                    try
                    {
                        await message.WriteAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"GRPC NetworkLoop exception: Message type {message.GetMessageType()} / " + e);
                        await Task.Delay(10);
                    }
                }
            }
        }
    }
}