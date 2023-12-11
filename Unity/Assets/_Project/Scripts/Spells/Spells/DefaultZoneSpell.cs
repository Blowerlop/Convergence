using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Spells
{
    public class DefaultZoneSpell : Spell
    {
        [SerializeField] private DefaultZoneSpellData spellData;
        DefaultZoneResults _results;
        
        [SerializeField] private LayerMask _layerMask;

        private Sequence _moveSeq;
        
        public override void Init(IChannelingResult channelingResult)
        {
            if (channelingResult is not DefaultZoneResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultZoneSpell)}!");
                return;
            }

            _results = results;
        }

        public override (Vector3, Quaternion) GetDefaultTransform(IChannelingResult channelingResult)
        {
            if (channelingResult is not DefaultZoneResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultZoneSpell)}!");
                return default;
            }
            
            return (results.Position, Quaternion.identity);
        }
        
        public void CheckForDamage()
        {
            if (!IsServer && !IsHost) return;

            var hits = Physics.OverlapSphere(_results.Position, spellData.zoneRadius, _layerMask);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out IDamageable damageable))
                    {
                        damageable.Damage(1);
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
