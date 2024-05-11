namespace Project.Effects
{
    public class HealEffect : Effect
    {
        public override EffectType Type => EffectType.Good;
       
        public int HealAmount;

        [Server]
        protected override bool TryApply_Internal(IEffectable effectable)
        {
            var entity = effectable.AffectedEntity;
            
            entity.Heal(HealAmount);
            return true;
        }

        public override void KillEffect() { }
        
        public override Effect GetInstance()
        {
            return this;
        }
    }
}