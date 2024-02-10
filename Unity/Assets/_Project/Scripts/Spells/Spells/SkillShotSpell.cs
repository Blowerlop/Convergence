using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class SkillShotSpell : Spell
    {
        [SerializeField] private Vector3 _castOffset;
        [SerializeField] private float _castRadius;
        
        SingleVectorResults _results;
        
        [SerializeField] private LayerMask _layerMask;

        private Sequence _moveSeq;
        
        protected override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return;
            }

            _results = results;
            
            _moveSeq = DOTween.Sequence();
            _moveSeq.Join(transform.DOMove(transform.position + results.VectorProp * 0.1f, 10f).SetEase(Ease.Linear));
            _moveSeq.OnComplete(() => NetworkObject.Despawn());
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return default;
            }
            
            return (player.PlayerTransform.position, Quaternion.LookRotation(results.VectorProp));
        }

        private void Update()
        {
            if (!IsServer && !IsHost) return;

            if (IsColliding(out var hit)) OnCollision(hit);
        }
        
        private bool IsColliding(out RaycastHit hit)
        {
            var forward = transform.forward;
            Vector3 realCastOffset = new Vector3(forward.x * _castOffset.x, 0, forward.z * _castOffset.z);
            
            return Physics.SphereCast(transform.position + realCastOffset, _castRadius, _results.VectorProp, 
                    out hit, 0.5f, _layerMask);
        }

        private void OnCollision(RaycastHit hit)
        {
            if (!hit.transform.TryGetComponent(out IDamageable damageable)) return;

            if (damageable.TryDamage(Data.baseDamage, CasterTeamIndex))
            {
                KillSpell();
            }
        }

        private void KillSpell()
        {
            _moveSeq.Kill();
            NetworkObject.Despawn();
        }
    }
}
