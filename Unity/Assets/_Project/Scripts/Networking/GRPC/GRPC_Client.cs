using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class GRPC_Client : NetworkBehaviour
    {
        private readonly GRPC_NetworkVariable<int> _networkVariableTest = new GRPC_NetworkVariable<int>("test");
        
        private void OnDisable()
        {
            if (GRPC_Rtt.isBeingDestroyed) return;
        }


        [ContextMenu(nameof(UpdateNetworkVariable))]
        private void UpdateNetworkVariable()
        {
            _networkVariableTest.Value += 1;
        }
    }
}