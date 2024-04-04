namespace Project.Effects
{
    public class CleanseEffect : Effect
    {
        public override EffectType Type => EffectType.Good;
        
        protected override bool TryApply_Internal(IEffectable effectable)
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