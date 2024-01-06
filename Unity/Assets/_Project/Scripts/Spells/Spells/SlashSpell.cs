using System;
using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class SlashSpell : Spell
    {
        [SerializeField] private FacingZoneSpellData spellData;
        [SerializeField] private BoxCollider collision;
        [SerializeField] private LayerMask layerMask;
        
        SingleVectorResults _results;
        
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

            Collider[] overlapResult = new Collider[2];
            Physics.OverlapBoxNonAlloc(transform.position + collision.center, collision.size / 2, overlapResult,
                collision.transform.rotation, layerMask);

            foreach (var hit in overlapResult)
            {
                if (hit != null && hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.Damage(1);
                }
            }
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(ZoneSpell)}!");
                return default;
            }
            
            var dir = results.VectorProp - player.PlayerTransform.position;
            dir.y = 0;
            dir.Normalize();
            
            return (results.VectorProp, Quaternion.LookRotation(dir));
        }
    }
}
