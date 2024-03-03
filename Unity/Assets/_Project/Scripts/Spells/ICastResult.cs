using System;
using System.Collections;
using System.Linq;
using GRPCClient;
using Sirenix.OdinInspector;
using Unity.Netcode;

namespace Project.Spells
{
    public interface ICastResult : INetworkSerializable
    {
        public bool TryFromCastRequest(GRPC_SpellCastRequest request);
        
        public static IEnumerable AllResultTypesAsString = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p != typeof(ICastResult) && typeof(ICastResult).IsAssignableFrom(p))
            .Select(p => new ValueDropdownItem() {Text = p.Name, Value = p.FullName});
    }
}