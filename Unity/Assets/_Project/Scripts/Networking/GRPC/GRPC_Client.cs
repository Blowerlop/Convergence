using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class GRPC_Client : NetworkBehaviour
    {
        private readonly GRPC_NetworkVariable<int> _name = new GRPC_NetworkVariable<int>("Name");
        private readonly GRPC_NetworkVariable<int> _health = new GRPC_NetworkVariable<int>("Health");
        private readonly GRPC_NetworkVariable<Vector3> _position = new GRPC_NetworkVariable<Vector3>("Position");
        private readonly GRPC_NetworkVariable<int> _currentAnimation = new GRPC_NetworkVariable<int>("CurrentAnimation");
        private readonly GRPC_NetworkVariable<int> _test = new GRPC_NetworkVariable<int>("test");
        // private readonly GRPC_NetworkVariable<Vector3> _networkVariableOpen = new GRPC_NetworkVariable<Vector3>("open");
        
        private void OnDisable()
        {
            if (GRPC_Rtt.isBeingDestroyed) return;
        }
 

        [ContextMenu(nameof(UpdateNetworkVariable))]
        private void UpdateNetworkVariable()
        {
            _name.Value += 1;
        }
    }
}