using System;
using DG.Tweening;
using Project._Project.Scripts.Managers;
using UnityEngine;

namespace Project.Spells
{
    public class SlashSpell : Spell
    {
        private FacingZoneSpellData _facingZoneData = null;
        private FacingZoneSpellData FacingZoneData => _facingZoneData ??= Data as FacingZoneSpellData;
        
        [SerializeField] private BoxCollider collision;
        [SerializeField] private LayerMask layerMask;
        
        private Sequence _moveSeq;
        
        protected override void Init(ICastResult castResult)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(ZoneSpell)}!");
                return;
            }
            
            SoundManager.instance.PlayStaticSound(Data.spellId, gameObject, SoundManager.EventType.SFX);
            
            CheckForDamage(GetCollisions());
            Invoke(nameof(KillSpell), 3);
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

        private Collider[] GetCollisions()
        {
            Collider[] overlapResult = new Collider[2];
            Physics.OverlapBoxNonAlloc(transform.position + collision.center, collision.size / 2, overlapResult,
                collision.transform.rotation, layerMask);

            return overlapResult;
        }

        private void CheckForDamage(Collider[] colliders)
        {
            foreach (var hit in colliders)
            {
                if (hit != null && hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TryDamage(Data.baseDamage, CasterTeamIndex);
                }
            }
        }

        [Server]
        private void KillSpell()
        {
            if (NetworkObject == null) return;
            
            NetworkObject.Despawn();
        }
    }
}
