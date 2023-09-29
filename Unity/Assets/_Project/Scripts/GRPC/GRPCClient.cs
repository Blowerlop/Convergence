using UnityEngine;

namespace Project
{
    public class GRPCClient : MonoBehaviour
    {
        private async void Start()
        {
            GRPC_NetworkManager.instance.StartClient();
        }
    }
}