using Unity.Netcode;

namespace Project
{
    public class GRPC_NetworkVariable<T> : NetworkVariable<T>
    {
        public GRPC_NetworkVariable(T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
    
        }
    }
}
