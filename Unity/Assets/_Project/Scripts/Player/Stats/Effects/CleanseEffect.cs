namespace Project.Effects
{
    public class CleanseEffect : Effect
    {
        public override EffectType Type => EffectType.Good;
        
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier)
        {
            effectable.SrvCleanse();
            return true;
        }

        public override void KillEffect() { }
        
        public override Effect GetInstance()
        {
            return this;
        }
    }
}