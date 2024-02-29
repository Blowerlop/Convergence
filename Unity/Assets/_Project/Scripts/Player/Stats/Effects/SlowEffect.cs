namespace Project.Effects
{
    public class SlowEffect : Effect 
    {
        public float SlowAmount;
        public float Duration;
        
        [Server]
        public override void Apply(PlayerRefs player)
        {
            // Get Move speed decrease it then increase it after duration
        }
    }
}