using Unity.Netcode;

namespace Project
{
    public class GrpcNetworkVariable<T> : NetworkVariable<T>
    {
        public GrpcNetworkVariable(T value = default,
            NetworkVariableReadPermission readPerm = DefaultReadPerm,
            NetworkVariableWritePermission writePerm = DefaultWritePerm) : base(value, readPerm, writePerm)
        {
    
        }
    }
}
