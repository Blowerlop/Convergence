using Project._Project.Scripts;

namespace Project.Effects
{
    public class HealEffect : Effect
    {
        public int HealAmount;
        
        [Server]
        public override bool TryApply(Entity entity)
        {
            entity.Heal(HealAmount);
            return true;
        }
    }
}