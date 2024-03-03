using DG.Tweening;
using Project._Project.Scripts.Managers;
using Project.Extensions;
using UnityEngine;

namespace Project.Spells
{
    public class SelfTargetSpell : Spell
    {
        [SerializeField] private float duration = 2f;
        
        protected override void Init(ICastResult castResult)
        {
            if (castResult is not EmptyResults)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SelfTargetSpell)}!");
                return;
            }

            if (Caster is not PCPlayerRefs pcPlayerRefs)
            {
                Debug.LogError($"Can't cast {nameof(SelfTargetSpell)} on a player that is not a PC!");
                return;
            }
            
            TryApplyEffects(pcPlayerRefs);

            StartCoroutine(Utilities.WaitForSecondsAndDoActionCoroutine(duration, () => NetworkObject.Despawn()));
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not EmptyResults)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SelfTargetSpell)}!");
                return default;
            }
            
            return (player.PlayerTransform.position, Quaternion.identity);
        }

        public override Vector3 GetDirection(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not EmptyResults)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(SelfTargetSpell)}!");
                return default;
            }
            
            return player.PlayerTransform.forward;
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            SoundManager.instance.PlayStaticSound(Data.spellId, gameObject, SoundManager.EventType.SFX);
        }
    }
}
