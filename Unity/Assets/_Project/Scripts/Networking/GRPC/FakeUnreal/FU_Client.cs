using System;
using Sirenix.OdinInspector;
using Unity.Netcode;

namespace Project
{
    public class FU_Client : NetworkBehaviour
    {
        [ShowInInspector] private FU_NetworkVariableReadOnly<int> _networkVariableReadOnly = new FU_NetworkVariableReadOnly<int>("Health");

        private void Start()
        {
            _networkVariableReadOnly.Initialize(this);
        }
    }
}
