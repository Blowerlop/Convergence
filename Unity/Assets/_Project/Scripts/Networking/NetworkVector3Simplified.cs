using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public struct NetworkVector3Simplified : INetworkSerializable, IEquatable<NetworkVector3Simplified>
    {
        [ReadOnly] public float x;
        [ReadOnly] public float y;
        [ReadOnly] public float z;

        public NetworkVector3Simplified(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }
        public NetworkVector3Simplified(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
            
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out x);
                reader.ReadValueSafe(out y);
                reader.ReadValueSafe(out z);
            }
            else
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(x);
                writer.WriteValueSafe(y);
                writer.WriteValueSafe(z);
            }
        }

        public bool Equals(NetworkVector3Simplified other)
        {
            return (Math.Abs(other.x - x) < 0.001f && Math.Abs(other.y - y) < 0.001f && Math.Abs(other.z - z) < 0.001f);
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

    }
}
