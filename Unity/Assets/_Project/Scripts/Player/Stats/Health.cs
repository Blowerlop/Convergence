using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class Health : NetworkBehaviour, IStat
    {
        private readonly GRPC_NetworkVariable<int> _maxHealth = new GRPC_NetworkVariable<int>("MaxHealth");
        private readonly GRPC_NetworkVariable<int> _health = new GRPC_NetworkVariable<int>("Health");
        
        /// <summary>
        /// Current health / Max health
        /// </summary>
        public event Action<int, int> OnValueChanged;

        [field: SerializeField, MinValue(0)] public int DefaultValue { get; set; } = 100;

        [ShowInInspector, ReadOnly] public int Value
        {
            get => _health.Value;
            set => _health.Value = Mathf.Clamp(value, 0, _maxHealth.Value);
        }

        [ShowInInspector, ReadOnly] public int MaxValue
        {
            get => _maxHealth.Value;
            set => _maxHealth.Value = value;
        }
        
        [SerializeField] private bool _debug;

    
        public override void OnNetworkSpawn()
        {
            _health.Initialize();
            _maxHealth.Initialize();

            SetToMaxValue();
        
            _health.OnValueChanged += HealthValueChanged;
            _maxHealth.OnValueChanged += HealthValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        
            _health.Reset();
            _maxHealth.Reset();
        
            _health.OnValueChanged -= HealthValueChanged;
            _maxHealth.OnValueChanged -= HealthValueChanged;
        }
    
        private void HealthValueChanged(int _, int __)
        {
            if (_debug) Debug.Log($"[Player {OwnerClientId}] Health value changed : Hp {_health.Value} / Max health {_maxHealth.Value}");
            OnValueChanged?.Invoke(_health.Value, _maxHealth.Value);
        }

        public void SetToMaxValue() => Value = MaxValue;

        [Button]
        private void AddHealth(int value)
        {
            Value += value;
        }
    }
}
