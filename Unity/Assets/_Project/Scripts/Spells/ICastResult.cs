using System;
using GRPCClient;
using Unity.Netcode;

namespace Project.Spells
{
    public interface ICastResult : INetworkSerializable
    {
        public bool TryFromCastRequest(GRPC_SpellCastRequest request);
        
        public static ICastResult TypeToInstance(CastResultType type)
        {
            return type switch
            {
                CastResultType.SingleVector => new SingleVectorResults(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}