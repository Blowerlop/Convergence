using System;
using Mewlist.Weaver;
using Unity.Netcode;

namespace Project
{
    public class ServerWeaver : IWeaver
    {
        private static void ControlMethodExecution()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening &&
                NetworkManager.Singleton.IsServer == false)
            {
                throw new Exception("You cannot call server method from a client");
            }
        }

        public void Weave(AssemblyInjector assemblyInjector)
        {
            assemblyInjector
                .OnMainAssembly()
                .OnAttribute<ServerAttribute>()
                .BeforeDo(ControlMethodExecution)
                .Inject();
        }
    }
}
