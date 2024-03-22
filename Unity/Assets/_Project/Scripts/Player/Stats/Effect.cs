using Project._Project.Scripts;

namespace Project
{
    [System.Serializable]
    public abstract class Effect
    {
        public abstract bool TryApply(Entity entity);
    }
}