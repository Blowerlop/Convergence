namespace Project.Effects
{
    public class HealEffect : Effect
    {
        public int HealAmount;
        
        [Server]
        public override void Apply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return;
            
            pcPlayer.Entity.Stats.health.Value += HealAmount;
        }
    }
}