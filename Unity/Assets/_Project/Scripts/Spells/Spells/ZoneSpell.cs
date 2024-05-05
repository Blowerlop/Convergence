using DG.Tweening;
using Project._Project.Scripts;
using Project._Project.Scripts.Managers;
using UnityEngine;

namespace Project.Spells
{
    public class ZoneSpell : Spell
    {
        private ZoneSpellData _zoneData = null;
        private ZoneSpellData ZoneData => _zoneData ??= Data as ZoneSpellData;
        
        SingleVectorResults _results;
        
        [SerializeField] private LayerMask _layerMask;
        
        private Sequence _moveSeq;

        protected override void Init(ICastResult castResult)
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

        public override Vector3 GetDirection(ICastResult castResult, PlayerRefs player)
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
            
            return dir;
        }

        public void CheckForEffects()
        {
            if (!IsServer && !IsHost) return;

            var hits = Physics.OverlapSphere(_results.VectorProp, ZoneData.zoneRadius, _layerMask);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out Entity entity))
                    {
                        TryApplyEffects(entity);
                    }
                }
            }
        }

        public void AnimEnd()
        {
            if (!IsServer && !IsHost) return;
            
            NetworkObject.Despawn();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            SoundManager.instance.PlayStaticSound(Data.spellId, gameObject, SoundManager.EventType.SFX);
        }
    }
}
