using System;
using Sirenix.OdinInspector;
using Unity.Netcode;

namespace Project
{
    [Serializable]
    public struct NetworkString : INetworkSerializable, IEquatable<NetworkString>
    {
        [ReadOnly] public string value;

        public NetworkString(string value)
        {
            this.value = value;
        }
            
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out value);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(value);
            }
        }

        public bool Equals(NetworkString other) =>
            String.Equals(other.value, value, StringComparison.CurrentCultureIgnoreCase);
    }
}
