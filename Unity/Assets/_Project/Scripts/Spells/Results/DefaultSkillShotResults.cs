using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    public struct DefaultSkillShotResults : IChannelingResult
    {
        public Vector3 Direction;
        
        public override string ToString()
        {
            return $"DefaultSkillShotResults: Direction: {Direction}";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Direction);
        }
    }
}