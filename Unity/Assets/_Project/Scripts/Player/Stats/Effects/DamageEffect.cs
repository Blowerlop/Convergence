namespace Project.Effects
{
    public class DamageEffect : Effect
    {
        public override EffectType Type => EffectType.Bad;
        
        public int DamageAmount;
        
        [Server]
        protected override bool TryApply_Internal(IEffectable effectable)
        {
            var entity = effectable.AffectedEntity;
            
            if (!entity.CanDamage(entity.TeamIndex))
            {
                entity.Damage(DamageAmount);
                return true;
            }

            return false;
        }

        public override void KillEffect() { }
        
        public override Effect GetInstance()
        {
            return this;
        }
    }
}