using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

namespace Project._Project.Scripts
{
    public class Dummy : NetworkBehaviour, IDamageable, IHealable
    {
        [SerializeField] private int maxHealth;
        
        private NetworkVariable<int> _health = new NetworkVariable<int>();

        private Sequence _shakeSeq;
        
        public void Damage(int modifier)
        {
            _health.Value -= modifier;
            if (_health.Value < 0) _health.Value = 0;
        }

        public bool CanDamage(int attackerTeamIndex)
        {
            return true;
        }

        public void Heal(int modifier)
        {
            _health.Value += modifier;
            if (_health.Value > maxHealth) _health.Value= maxHealth;
        }

        public void MaxHeal() => Heal(maxHealth);

        public override void OnNetworkSpawn()
        {
            if (IsServer || IsHost) _health.Value = maxHealth;
            
            _health.OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            _health.OnValueChanged -= OnHealthChanged;
        }
        
        private void OnHealthChanged(int oldValue, int newValue)
        {
            if(_shakeSeq != null && _shakeSeq.IsActive()) _shakeSeq.Kill(complete: true);
            
            _shakeSeq = DOTween.Sequence();
            _shakeSeq.Join(transform.DOShakeRotation(0.35f, Vector3.one * 5f, 15, 90f, true));
        }
    }
}