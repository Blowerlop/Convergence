using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public struct EmptyResults : ICastResult
    {
        public override string ToString()
        {
            return "EmptyResults";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter { }

        public bool TryFromCastRequest(GRPC_SpellCastRequest request) => true;
    }
}