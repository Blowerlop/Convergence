using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public struct DefaultZoneResults : IChannelingResult
    {
        public Vector3 Position;
        
        public override string ToString()
        {
            return $"DefaultZoneResults: Position: {Position}";
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
        }
    }
}