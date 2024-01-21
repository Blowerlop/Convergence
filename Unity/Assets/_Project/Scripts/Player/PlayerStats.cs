using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class PlayerStats : NetworkBehaviour, IDamageable, IHealable
    {
        [ShowInInspector] private GRPC_NetworkVariable<int> _maxHealth = new GRPC_NetworkVariable<int>("MaxHealth");
        [ShowInInspector] private GRPC_NetworkVariable<int> _health = new GRPC_NetworkVariable<int>("Health");
        
        public event Action<int, int> OnHealthChanged; 
        
        // + Movement speed / attack speed / damage scale / other stats...
        
        //----
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _health.Initialize();

            _health.OnValueChanged += HealthValueChanged;
            _maxHealth.OnValueChanged += MaxHealthValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            _health.OnValueChanged -= HealthValueChanged;
            _maxHealth.OnValueChanged -= MaxHealthValueChanged;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _health.Reset();
            _maxHealth.Reset();
        }
        
        [Server]
        public void ServerInit(SOCharacter character)
        {
            _maxHealth.Value = character.BaseHealth;
            _health.Value = character.BaseHealth;
        }

        private void MaxHealthValueChanged(int _, int newValue)
        {
            OnHealthChanged?.Invoke(_health.Value, newValue);
        }        
        
        private void HealthValueChanged(int _, int newValue)
        {
            OnHealthChanged?.Invoke(newValue, _maxHealth.Value);
        }

        [Server]
        public void Damage(int modifier)
        {
            int newValue = _health.Value - modifier;

            newValue = Mathf.Clamp(newValue, 0, _maxHealth.Value);
            _health.Value = newValue;
        }

        [Server]
        public void Heal(int modifier)
        {
            int newValue = _health.Value + modifier;

            newValue = Mathf.Clamp(newValue, 0, _maxHealth.Value);
            _health.Value = newValue;
        }
    }
}