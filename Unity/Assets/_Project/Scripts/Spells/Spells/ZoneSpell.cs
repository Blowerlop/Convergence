using System;
using DG.Tweening;
using Project._Project.Scripts;
using Project._Project.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Spells
{
    public class ZoneSpell : Spell
    {
        private ZoneSpellData _zoneData = null;
        private ZoneSpellData ZoneData => _zoneData ??= Data as ZoneSpellData;
        
        [SerializeField] private ApplyType applyType;
        [SerializeField] private KillType killType;

        [SerializeField, ShowIf(nameof(IsTimed))] private float duration;
        
        private SingleVectorResults _results;
        
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
            
            if (applyType == ApplyType.OnStart)
                CheckForEffects();
            
            if (killType == KillType.Timed)
                StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(duration, AnimEnd));
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

        [Server]
        public void CheckForEffects()
        {
            if (!IsServer && !IsHost) return;

            var hits = Physics.OverlapSphere(_results.VectorProp, ZoneData.zoneRadius, Constants.Layers.EntityMask);
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

        [Server]
        public void AnimEnd()
        {
            NetworkObject.Despawn();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            SoundManager.instance.PlayStaticSound(Data.spellId, gameObject, SoundManager.EventType.SFX);
        }
        
        private enum ApplyType
        {
            AnimationDriven,
            OnStart
        }
        
        private enum KillType
        {
            AnimationDriven,
            Timed
        }
        
        private bool IsTimed() => killType == KillType.Timed;
    }
}
