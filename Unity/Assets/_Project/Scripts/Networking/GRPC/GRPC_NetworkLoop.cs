using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Project
{
    public class GRPC_Message
    {
        private readonly Func<Task> _message;
        
        public GRPC_Message(Func<Task> cache)
        {
            _message = cache;
        }
            
        public Task WriteAsync()
        {
            return _message.Invoke();
        }
    }
    
    public class GRPC_NetworkLoop : NetworkSingleton<GRPC_NetworkLoop>, INetworkUpdateSystem 
    {
        private static readonly Stack<GRPC_Message> _messages;

        
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
                while (_messages.Any())
                {
                    await _messages.Pop().WriteAsync();
                }
            }
        }
    }
}