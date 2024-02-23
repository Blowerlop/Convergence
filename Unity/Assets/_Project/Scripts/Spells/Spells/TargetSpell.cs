using DG.Tweening;
using UnityEngine;

namespace Project.Spells
{
    public class TargetSpell : Spell
    {
        [SerializeField] private float speed = 3f;
        
        private Transform _target;
        
        protected override void Init(ICastResult castResult)
        {
            if (castResult is not SingleUIntResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(TargetSpell)}!");
                return;
            }
            
            _target = NetworkManager.SpawnManager.SpawnedObjects[results.UIntProp].transform;
        }

        public override (Vector3, Quaternion) GetDefaultTransform(ICastResult castResult, PlayerRefs player)
        {
            if (castResult is not SingleUIntResults results)
            {
                Debug.LogError(
                    $"Given channeling result {nameof(castResult)} is not the required type for {nameof(TargetSpell)}!");
                return default;
            }
            
            return (player.PlayerTransform.position, Quaternion.identity);
        }

        private void Update()
        {
            if (!IsServer && !IsHost) return;

            transform.position = Vector3.Lerp(transform.position, _target.position, speed * Time.deltaTime);

            if ((transform.position - _target.position).sqrMagnitude <= 0.1f)
            {
                OnCollision();
            }
        }

        private void OnCollision()
        {
            if (_target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TryDamage(Data.baseDamage, CasterTeamIndex);
            }
            
            KillSpell();
        }

        private void KillSpell()
        {
            NetworkObject.Despawn();
        }
    }
}
