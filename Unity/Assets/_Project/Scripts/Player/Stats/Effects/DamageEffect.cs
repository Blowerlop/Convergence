using Project._Project.Scripts;
using UnityEngine;

namespace Project.Effects
{
    public enum DamageType
    {
        Default,
        MissingHealthPercentage
    }
    
    public class DamageEffect : Effect
    {
        public override EffectType Type => EffectType.Bad;
        
        public DamageType DamageType;
        
        public int DamageAmount;
        
        [Server]
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
        {
            var entity = effectable.AffectedEntity;
            if (!entity.CanDamage(applier.TeamIndex)) return false;
            
            int amount;

            switch (DamageType)
            {
                case DamageType.Default:
                    amount = DamageAmount;
                    break;
                case DamageType.MissingHealthPercentage:
                    if (!entity.Stats.TryGet<HealthStat>(out var health))
                        return false;
                        
                    var missingHealth = health.maxValue - health.value;
                        
                    amount = missingHealth * DamageAmount / 100;
                    Debug.Log("Missing health: " + missingHealth + " Damage amount: " + amount + " Damage percentage: " + DamageAmount + "%");
                    break;
                default:
                    return false;
            }

            if (HealthTextPool.instance)
            {
                var dir = entity.transform.position - applier.transform.position;
                dir.y = 0;
                dir.Normalize();
            
                HealthTextPool.instance.RequestText(-amount, effectable.AffectedEntity.transform, dir);
            }

            entity.Damage(amount);
            return true;
        }

        public override void KillEffect() { }
        
        public override Effect GetInstance()
        {
            return this;
        }
    }
}