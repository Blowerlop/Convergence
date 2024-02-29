namespace Project.Effects
{
    public class DamageEffect : Effect
    {
        public int DamageAmount;
        
        [Server]
        public override void Apply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return;
            
            pcPlayer.Entity.Stats.health.Value -= DamageAmount;
        }
    }
}