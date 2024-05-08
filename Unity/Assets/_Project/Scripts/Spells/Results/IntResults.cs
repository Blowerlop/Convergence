using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public struct IntResults : ICastResult
    {
        public int IntProp;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IntProp);
        }

        public bool TryFromCastRequest(GRPC_SpellCastRequest request)
        {
            /*if(request. is not { Count: 1 })
                return false;
            
            IntProp = ;*/
            return true;
        }
    }
}