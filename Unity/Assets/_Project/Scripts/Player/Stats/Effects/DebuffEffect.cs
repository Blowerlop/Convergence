namespace Project.Effects
{
    public class DebuffEffect : Effect
    {
        public override EffectType Type => EffectType.Bad;
        
        protected override bool TryApply_Internal(IEffectable effectable)
        {
            effectable.SrvDebuff();
            return true;
        }

        public override void KillEffect() { }
        
        public override Effect GetInstance()
        {
            return this;
        }
    }
}