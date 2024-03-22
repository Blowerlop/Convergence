using Project._Project.Scripts;

namespace Project.Effects
{
    public class DamageEffect : Effect
    {
        public int DamageAmount;
        
        [Server]
        public override bool TryApply(Entity entity)
        {
            if (!entity.CanDamage(entity.TeamIndex))
            {
                entity.Damage(DamageAmount);
                return true;
            }

            return false;
        }
    }
}