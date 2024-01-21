#if UNITY_EDITOR
using Mewlist.Weaver;

namespace Project
{
    public class ParrelSyncWeaver : IWeaver
    {
        public static void ControlMethodExecution()
        {
            if (ParrelSync.ClonesManager.IsClone()) return;
        }

        public void Weave(AssemblyInjector assemblyInjector)
        {
            assemblyInjector
                .OnMainAssembly()
                .OnAttribute<ParrelSyncIgnoreAttribute>()
                .BeforeDo(ControlMethodExecution)
                .Inject();
        }
    }
}
#endif 