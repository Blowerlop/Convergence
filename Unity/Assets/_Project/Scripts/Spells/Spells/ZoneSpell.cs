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
        
        [SerializeField] private HitZoneShape hitZoneShape;
        
        [SerializeField, ShowIf(nameof(IsBoxShape))] private BoxCollider boxCollider;
        
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
                StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(duration, KillSpell));
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleVectorResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(ZoneSpell)}!");
                return default;
            }

            Quaternion rotation = Quaternion.identity;

            if (ZoneData.lookAtCenter)
                rotation = Quaternion.LookRotation(GetDirection(castResult, player));
            
            return (results.VectorProp, rotation);
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

            var results = new Collider[5];

            var size = hitZoneShape switch
            {
                HitZoneShape.Default => Physics.OverlapSphereNonAlloc(_results.VectorProp, ZoneData.zoneRadius, results,
                    Constants.Layers.EntityMask),
                HitZoneShape.Box => Physics.OverlapBoxNonAlloc(transform.position + boxCollider.center,
                    boxCollider.size / 2, results, boxCollider.transform.rotation, Constants.Layers.EntityMask),
                _ => throw new ArgumentOutOfRangeException()
            };

            for(int i = 0; i < size; i++)
            {
                if(results[i].TryGetComponent(out Entity entity))
                {
                    TryApplyEffects(entity);
                }
            }
        }

        [Server]
        public void AnimEnd()
        {
            KillSpell();
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

        private enum HitZoneShape
        {
            Default,
            Box
        }
        
        private bool IsTimed() => killType == KillType.Timed;
        private bool IsBoxShape() => hitZoneShape == HitZoneShape.Box;
    }
}
