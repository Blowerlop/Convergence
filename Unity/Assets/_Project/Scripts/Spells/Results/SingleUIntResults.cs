using GRPCClient;
using Unity.Netcode;

namespace Project.Spells
{
    public struct SingleUIntResults : ICastResult
    {
        public uint UIntProp;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref UIntProp);
        }

        public bool TryFromCastRequest(GRPC_SpellCastRequest request)
        {
            if(request.UintParams is not { Count: 1 })
                return false;

            UIntProp = request.UintParams[0];
            return true;
        }
    }
}