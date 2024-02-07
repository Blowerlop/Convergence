using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class RTT : NetworkBehaviour
    {
        [SerializeField] private float _pingInterval = 0.5f;
        [ShowInInspector, ReadOnly] private float _currentRTT;
        private ClientRpcParams _clientRpcParams;
        private float _lastPingTime = 0.0f;
        
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                enabled = false;
                return;
            }
            
            _clientRpcParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { NetworkManager.Singleton.LocalClientId } }
            };
        }
        
        
        private void FixedUpdate()
        {
            if (Time.realtimeSinceStartup - _lastPingTime > _pingInterval)
            {
                _lastPingTime = Time.realtimeSinceStartup;
                PingServerRpc(_lastPingTime);
            }
            
        }
        

        [ServerRpc(RequireOwnership = false)]
        private void PingServerRpc(float pingTime)
        {
            PongClientRpc(pingTime, _clientRpcParams);
        }

        [ClientRpc]
        private void PongClientRpc(float pingTime, ClientRpcParams clientRpcParams = default)
        {
            _currentRTT = Mathf.Round((Time.realtimeSinceStartup - pingTime) * 1000);
        }
    }
}