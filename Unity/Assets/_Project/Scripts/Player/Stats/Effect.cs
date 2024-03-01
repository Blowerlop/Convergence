namespace Project
{
    [System.Serializable]
    public abstract class Effect
    {
        public abstract bool TryApply(PlayerRefs player);
    }
}