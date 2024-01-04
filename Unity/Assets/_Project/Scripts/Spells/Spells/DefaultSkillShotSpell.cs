using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class DefaultSkillShotSpell : Spell
    {
        [SerializeField] private Vector3 _castOffset;
        [SerializeField] private float _castRadius;
        
        DefaultSkillShotResults _results;
        
        [SerializeField] private LayerMask _layerMask;

        private Sequence _moveSeq;
        
        public override void Init(IChannelingResult channelingResult)
        {
            if (channelingResult is not DefaultSkillShotResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultSkillShotSpell)}!");
                return;
            }

            _results = results;
            
            _moveSeq = DOTween.Sequence();
            _moveSeq.Join(transform.DOMove(transform.position + results.Direction * 6.75f, 0.35f).SetEase(Ease.Linear));
            _moveSeq.OnComplete(() => NetworkObject.Despawn());
        }

        public override (Vector3, Quaternion) GetDefaultTransform(IChannelingResult channelingResult, PlayerRefs player)
        {
            if (channelingResult is not DefaultSkillShotResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(channelingResult)} is not the required type for {nameof(DefaultSkillShotSpell)}!");
                return default;
            }
            
            //TODO: Find a way to get the right player's position.
            return (player.PlayerTransform.position, Quaternion.LookRotation(results.Direction));
        }

        private void Update()
        {
            if (!IsServer && !IsHost) return;

            var forward = transform.forward;
            Vector3 realCastOffset = new Vector3(forward.x * _castOffset.x, 0, forward.z * _castOffset.z);
            
            if (Physics.SphereCast(transform.position + realCastOffset, _castRadius, _results.Direction, 
                    out RaycastHit hit, 0.1f, _layerMask))
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
