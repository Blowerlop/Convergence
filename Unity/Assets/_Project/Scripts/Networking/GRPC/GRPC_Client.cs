using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class GRPC_Client : NetworkBehaviour
    {
        private readonly GRPC_NetworkVariable<int> _networkVariableTest = new GRPC_NetworkVariable<int>("test");
        // private readonly GRPC_NetworkVariable<Vector3> _networkVariableOpen = new GRPC_NetworkVariable<Vector3>("open");
        
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