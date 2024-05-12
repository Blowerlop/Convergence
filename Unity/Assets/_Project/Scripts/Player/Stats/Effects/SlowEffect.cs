using Project._Project.Scripts;
using UnityEngine;

namespace Project.Effects
{
    public class SlowEffect : Effect 
    {
        public int SlowAmount;
        public float Duration;

        public override EffectType Type => EffectType.Bad;

        private MoveSpeedStat _stat;
        private Coroutine _appliedCoroutine;
        
        private int _slowedValue;
        
        [Server]
        protected override bool TryApply_Internal(IEffectable effectable, int applierTeamIndex)
        {
            var entity = effectable.AffectedEntity;
            
            if (!entity.Stats.TryGet(out _stat))
            {
                Debug.LogWarning(
                    $"Can't apply SlowEffect on Entity {entity.data.name} because it doesn't have a MoveSpeedStat");
                return false;
            }
            
            _slowedValue = _stat.Slow(SlowAmount);

            _appliedCoroutine = AffectedEffectable.AffectedEntity.StartCoroutine(
                Utilities.WaitForSecondsAndDoActionCoroutine(Duration, RemoveSlow));
            
            return false;
        }

        public override void KillEffect()
        {
            RemoveSlow();
            AffectedEffectable.AffectedEntity.StopCoroutine(_appliedCoroutine);
        }
        
        private void RemoveSlow()
        {
            _stat.value += _slowedValue;
        }
        
        public override Effect GetInstance()
        {
            return new SlowEffect()
            {
                SlowAmount = SlowAmount, 
                Duration = Duration
            };
        }
    }
}