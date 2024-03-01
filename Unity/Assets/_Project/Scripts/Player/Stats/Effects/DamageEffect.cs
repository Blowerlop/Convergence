namespace Project.Effects
{
    public class DamageEffect : Effect
    {
        public int DamageAmount;
        
        [Server]
        public override bool TryApply(PlayerRefs player)
        {
            if(player is not PCPlayerRefs pcPlayer)
                return false;

            if (!pcPlayer.Entity.CanDamage(player.TeamIndex))
            {
                pcPlayer.Entity.Damage(DamageAmount);
                return true;
            }

            return false;
        }
    }
}