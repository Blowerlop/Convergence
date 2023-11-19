using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class DefaultSkillShot : Spell
    {
        public override void Init(IChannelingResult channelingResult)
        {
            if (channelingResult is not DefaultSkillShotResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultSkillShot)}!");
                return;
            }

            Sequence seq = DOTween.Sequence();
            seq.Join(transform.DOMove(transform.position + results.Direction * 6.75f, 0.5f).SetEase(Ease.Linear));
            seq.OnKill(() => NetworkObject.Despawn());
        }

        public override (Vector3, Quaternion) GetDefaultTransform(IChannelingResult channelingResult)
        {
            if (channelingResult is not DefaultSkillShotResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultSkillShot)}!");
                return default;
            }
            
            //TODO: Find a way to get the right player's position.
            return (Vector3.zero + Vector3.up * 1f, Quaternion.LookRotation(results.Direction));
        }
    }
}
