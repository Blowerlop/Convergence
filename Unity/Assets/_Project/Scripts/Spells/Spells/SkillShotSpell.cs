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
        
        public override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SkillShotSpell)}!");
                return;
            }

            _results = results;
            
            _moveSeq = DOTween.Sequence();
            _moveSeq.Join(transform.DOMove(transform.position + results.VectorProp * 6.75f, 0.35f).SetEase(Ease.Linear));
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

            var forward = transform.forward;
            Vector3 realCastOffset = new Vector3(forward.x * _castOffset.x, 0, forward.z * _castOffset.z);
            
            if (Physics.SphereCast(transform.position + realCastOffset, _castRadius, _results.VectorProp, 
                    out RaycastHit hit, 0.5f, _layerMask))
            {
                if (hit.transform.TryGetComponent(out IDamageable damageable))
                {
                    damageable.Damage(1);
                    _moveSeq.Kill();
                    NetworkObject.Despawn();
                }
            }
        }
    }
}
