using Sirenix.OdinInspector;

namespace Project
{
    public class FU_Client : FU_NetworkObject
    {
        [ShowInInspector] private FU_NetworkVariableReadOnly<int> _networkVariableReadOnly = new FU_NetworkVariableReadOnly<int>("Health");

        private void Start()
        {
            _networkVariableReadOnly.Initialize(this);
        }
    }
}
