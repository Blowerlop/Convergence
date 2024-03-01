namespace Project.Effects
{
    public class HealEffect : Effect
    {
        public int HealAmount;
        
        [Server]
        public override bool TryApply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return false;
            
            pcPlayer.Entity.Heal(HealAmount);
            return true;
        }
    }
}