using GRPCClient;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public struct SingleVectorResults : ICastResult
    {
        public Vector3 VectorProp;
        
        public override string ToString()
        {
            return $"SingleVectorResults: VectorProp: {VectorProp}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref VectorProp);
        }

        public bool TryFromCastRequest(GRPC_SpellCastRequest request)
        {
            if(request.VectorParams is not { Count: 1 })
                return false;
            
            VectorProp = Utilities.GrpcToUnityVector3(request.VectorParams[0]);
            return true;
        }
    }
}