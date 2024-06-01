using UnityEngine;

namespace Project.Effects
{
    public class RangeEffect : Effect
    {
        public override EffectType Type => EffectType.Neutral;
        
        [SerializeField] private float _rangeAmount;
        [SerializeField] private float _duration;
        
        private Coroutine _appliedCoroutine;

        private float _value;
        
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
        {
            if (!effectable.AffectedEntity.Stats.TryGet(out AttackRangeStat stat))
            {
                Debug.LogWarning(
                    $"Can't apply RangeEffect on Entity {effectable.AffectedEntity.data.name} because it doesn't have a AttackRangeStat");
                return false;
            }
            
            _value = stat.AddRange(_rangeAmount);
            
            AddToEffectable();
            
            _appliedCoroutine = AffectedEffectable.AffectedEntity.StartCoroutine(
                Utilities.WaitForSecondsAndDoActionCoroutine(_duration, RemoveRange));

            return true;
        }

        public override void KillEffect()
        {
            RemoveFromEffectable();
            
            RemoveRange();
            AffectedEffectable.AffectedEntity.StopCoroutine(_appliedCoroutine);
        }

        public override Effect GetInstance()
        {
            return new RangeEffect() { _rangeAmount = _rangeAmount, _duration = _duration };
        }

        private void RemoveRange()
        {
            if (!AffectedEffectable.AffectedEntity.Stats.TryGet(out AttackRangeStat stat)) return;
            
            stat.value -= _value;
        }

        public override float GetEffectValue()
        {
            return _rangeAmount;
        }

        public override float GetEffectDuration()
        {
            return _duration; 
        }
    }
}