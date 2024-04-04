using Project._Project.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Effects
{
    public class ShieldEffect : Effect 
    {
        public override EffectType Type => EffectType.Good;
        
        public int ShieldAmount;
        public bool HasDuration;
        [ShowIf(nameof(HasDuration))] public float Duration;

        private int _shieldId;
        
        private Coroutine _appliedCoroutine;
        
        [Server]
        protected override bool TryApply_Internal(IEffectable effectable)
        {
            var entity = effectable.AffectedEntity;
            
            _shieldId = entity.Shield(ShieldAmount);

            if (!HasDuration) return true;

            AddToEffectable();
            
            _appliedCoroutine = AffectedEffectable.AffectedEntity.StartCoroutine(
                Utilities.WaitForSecondsAndDoActionCoroutine(Duration, RemoveShield));
            
            return true;
        }

        public override void KillEffect()
        {
            if (!HasDuration) return;

            Debug.Log("Kill shield effect");
            
            RemoveFromEffectable();
            
            RemoveShield();
            AffectedEffectable.AffectedEntity.StopCoroutine(_appliedCoroutine);
        }

        private void RemoveShield()
        {
            AffectedEffectable.AffectedEntity.UnShield(_shieldId);
        }
        
        public override Effect GetInstance()
        {
            return new ShieldEffect()
            {
                ShieldAmount = ShieldAmount, 
                HasDuration = HasDuration, 
                Duration = Duration
            };
        }
    }
}