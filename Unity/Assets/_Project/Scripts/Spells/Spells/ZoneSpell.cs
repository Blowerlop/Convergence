using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class ZoneSpell : Spell
    {
        [SerializeField] private ZoneSpellData spellData;
        SingleVectorResults _results;
        
        [SerializeField] private LayerMask _layerMask;

        private Sequence _moveSeq;
        
        public override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(ZoneSpell)}!");
                return;
            }

            _results = results;
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(ZoneSpell)}!");
                return default;
            }
            
            return (results.VectorProp, Quaternion.identity);
        }
        
        public void CheckForDamage()
        {
            if (!IsServer && !IsHost) return;

            var hits = Physics.OverlapSphere(_results.VectorProp, spellData.zoneRadius, _layerMask);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.Damage(10);
                    }
                }
            }
        }

        public void AnimEnd()
        {
            if (!IsServer && !IsHost) return;
            
            NetworkObject.Despawn();
        }
    }
}
