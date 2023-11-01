using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    [DefaultExecutionOrder(1)]
    public class TestGrpc : MonoBehaviour
    {
        private void OnEnable()
        {
            // NetworkManager.Singleton.OnServerStarted += GRPC_NetworkManager.instance.StartClient;
            // NetworkManager.Singleton.OnServerStopped += SingletonOnOnServerStopped;
        }

        private void OnDisable()
        {
            // if (NetworkManager.Singleton == null) return;
            //
            // NetworkManager.Singleton.OnServerStarted -= GRPC_NetworkManager.instance.StartClient;
            // NetworkManager.Singleton.OnServerStopped -= SingletonOnOnServerStopped;
        }

        [Button]
        public void StartALl()
        {
            NetworkManager.Singleton.StartServer();
        }
        
        private void SingletonOnOnServerStopped(bool _)
        {
            GRPC_NetworkManager.instance.StopClient();
        }
    }
}
