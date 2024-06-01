using System;
using Project._Project.Scripts;
using UnityEngine;

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
        
        public bool TryApply(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
        {
            AffectedEffectable = effectable;
            
            return TryApply_Internal(effectable, applier, applyPosition);
        }

        protected abstract bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition);
        
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

        public virtual float GetEffectValue() {  return 0; }

        public virtual float GetEffectDuration() { return 0; }
    }
}