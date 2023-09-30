using System;
using UnityEngine;

namespace Project
{
    public class GRPC_Client : MonoBehaviour
    {
        private void Start()
        {
            GRPC_NetworkManager.instance.StartClient();
        }

        private void OnEnable()
        {
            GRPC_Rtt.instance.onRttUpdateEvent.Subscribe(this, DebugRtt);
        }
        
        private void OnDisable()
        {
            if (GRPC_Rtt.isBeingDestroyed) return;
            
            GRPC_Rtt.instance.onRttUpdateEvent.Unsubscribe(DebugRtt);
        }
        
        
        private void DebugRtt(float value)
        {
            Debug.Log(value);
        }
    }
}