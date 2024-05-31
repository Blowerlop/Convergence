using UnityEngine;

namespace Project.Effects
{
    public class SilenceEffect : Effect
    {
        public override EffectType Type => EffectType.Bad;
        
        public float Duration;
        
        private Coroutine _appliedCoroutine;
        
        protected override bool TryApply_Internal(IEffectable effectable, PlayerRefs applier, Vector3 applyPosition)
        {
            effectable.AffectedEntity.Silence();
            
            AddToEffectable();
            
            _appliedCoroutine = AffectedEffectable.AffectedEntity.StartCoroutine(
                Utilities.WaitForSecondsAndDoActionCoroutine(Duration, RemoveSilence));

            return true;
        }

        public override void KillEffect()
        {
            RemoveFromEffectable();
            
            RemoveSilence();
            AffectedEffectable.AffectedEntity.StopCoroutine(_appliedCoroutine);
        }

        public override Effect GetInstance()
        {
            return new SilenceEffect() { Duration = Duration };
        }

        private void RemoveSilence()
        {
            AffectedEffectable.AffectedEntity.Unsilence();
        }
        public override float GetEffectDuration()
        {
            return Duration; 
        }
    }
}