using UnityEngine;

namespace Project.Effects
{
    public class HealEffect : Effect
    {
        public override EffectType Type => EffectType.Good;
       
        public int HealAmount;

        [Server]
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
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

        public override float GetEffectValue()
        {
            return HealAmount;
        }
    }
}