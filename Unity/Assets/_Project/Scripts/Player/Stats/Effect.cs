using System;
using Project._Project.Scripts;

namespace Project
{
    public enum EffectType
    {
        Good,
        Bad,
        Neutral
    }
    
    [System.Serializable]
    public abstract class Effect
    {
        public abstract EffectType Type { get; }
        
        protected IEffectable AffectedEffectable { get; private set; }
        
        public bool TryApply(IEffectable effectable)
        {
            AffectedEffectable = effectable;
            
            return TryApply_Internal(effectable);
        }

        protected abstract bool TryApply_Internal(IEffectable effectable);
        
        public abstract void KillEffect();
        
        protected void AddToEffectable()
        {
            AffectedEffectable.SrvAddEffect(this);
        }
        
        protected void RemoveFromEffectable()
        {
            AffectedEffectable.SrvRemoveEffect(this);
        }

        public abstract Effect GetInstance();
    }
}