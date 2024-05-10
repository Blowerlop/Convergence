using UnityEngine;

namespace Project.Spells
{
    public class BoomerangSpell : Spell
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float firstHalfDuration, secondHalfEaseDuration;

        [SerializeField] private AnimationCurve firstHalfEase, secondHalfEase;
        
        private SingleVectorResults _results;
        private float _timer;
        
        protected override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(BoomerangSpell)}!");
                return;
            }
            
            _results = results;
        }

        private void Update()
        {
            if (!IsServer) return;
            
            _timer += Time.deltaTime;
            
            if (_timer < firstHalfDuration)
            {
                var easeFactor = firstHalfEase.Evaluate(_timer / firstHalfDuration);
                transform.position += _results.VectorProp * (moveSpeed * easeFactor * Time.deltaTime);
            }
            else
            {
                var dir = Caster.PlayerTransform.position - transform.position;
                dir.y = 0;
                
                var normalizedDir = dir.normalized;

                var easeFactor = secondHalfEase.Evaluate((_timer - firstHalfDuration) / secondHalfEaseDuration);
                transform.position += normalizedDir * (moveSpeed * easeFactor * Time.deltaTime);
                
                if(dir.magnitude < 0.1f) 
                    KillSpell();
            }
        }
        
        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(BoomerangSpell)}!");
                return default;
            }
            
            return (player.PlayerTransform.position, Quaternion.LookRotation(results.VectorProp));
        }

        public override Vector3 GetDirection(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(BoomerangSpell)}!");
                return default;
            }
            
            return results.VectorProp;
        }
    }
}