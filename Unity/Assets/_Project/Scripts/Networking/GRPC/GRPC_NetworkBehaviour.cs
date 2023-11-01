using System;
using UnityEngine;

namespace Project
{
    public abstract class GRPC_NetworkBehaviour : MonoBehaviour, IDisposable
    {
        protected virtual void Start()
        {
            GRPC_NetworkManager.instance.Register(this);
        }
        
        protected virtual void OnDestroy()
        {
            GRPC_NetworkManager.instance.Unregister(this);
        }

        public virtual void CleanTokens()
        {
        }
        
        public abstract void Dispose();
    }
}
