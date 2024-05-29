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
            
            if (HealthTextPool.instance)
            {
                var dir = entity.transform.position - applier.transform.position;
                dir.y = 0;
                dir.Normalize();
            
                HealthTextPool.instance.RequestText(HealAmount, effectable.AffectedEntity.transform, dir);
            }
            
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