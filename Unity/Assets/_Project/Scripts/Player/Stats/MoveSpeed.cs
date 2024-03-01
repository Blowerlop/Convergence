using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Project
{
    public class MoveSpeed : NetworkBehaviour, IStat
    {
        private readonly GRPC_NetworkVariable<int> _moveSpeed = new GRPC_NetworkVariable<int>("MoveSpeed", 1);

        /// <summary>
        /// Current ms / maxms
        /// </summary>
        public event Action<int, int> OnValueChanged;

        [field: SerializeField, MinValue(0)] public int DefaultValue { get; set; } = 3;

        [ShowInInspector, ReadOnly] public int Value
        {
            get => _moveSpeed.Value;
            set => _moveSpeed.Value = Mathf.Clamp(value, 1, MaxValue);
        }

        [ShowInInspector, ReadOnly] public int MaxValue
        {
            get => 999;
            set { }
        }

        [SerializeField] private bool _debug;

        public override void OnNetworkSpawn()
        {
            _moveSpeed.Initialize();
            _moveSpeed.OnValueChanged += MoveSpeedValueChanged;        
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        
            _moveSpeed.Reset();
        
            _moveSpeed.OnValueChanged -= MoveSpeedValueChanged;
        }
    
        private void MoveSpeedValueChanged(int _, int __)
        {
            if (_debug) Debug.Log($"[Player {OwnerClientId}] MoveSpeed value changed : Ms {_moveSpeed.Value} / Max ms {MaxValue}");
            OnValueChanged?.Invoke(Value, MaxValue);
        }

        public void SetToMaxValue() => Value = MaxValue;

        public int Slow(int amount)
        {
            int lastMs = Value;
            
            Value -= amount;

            int newMs = Value;
            
            return lastMs - newMs;
        }
    }
}
