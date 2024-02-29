namespace Project
{
    [System.Serializable]
    public abstract class Effect
    {
        [Server]
        public abstract void Apply(PlayerRefs player);
    }
}