using System;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    /// <summary>
    /// Used by clients and server to determine if a spell can be casted.
    /// </summary>
    public class CooldownController : NetworkBehaviour
    {
        [SerializeField] private string cooldownNetVarPrefix = "Cooldown_";
        
        private GRPC_NetworkVariable<int>[] _cooldowns = new GRPC_NetworkVariable<int>[SpellData.CharacterSpellsCount];

        private GRPC_NetworkVariable<int> _cd1, _cd2, _cd3, _cd4;
        
        private readonly Timer[] _timers = new Timer[SpellData.CharacterSpellsCount];
        
        public event Action<int, float> OnLocalCooldownStarted; 
        public event Action<int, float> OnLocalCooldownUpdated; 
        
        public event Action<int, float> OnServerCooldownUpdated; 
        public event Action<int> OnServerCooldownEnded; 

        private void Awake()
        {
            CreateNetVarInstance();
            
            for(int i = 0; i < _timers.Length; i++)
            {
                _timers[i] = new Timer();
            }
        }

        private void CreateNetVarInstance()
        {
            _cd1 = new GRPC_NetworkVariable<int>(cooldownNetVarPrefix + 0);
            _cd2 = new GRPC_NetworkVariable<int>(cooldownNetVarPrefix + 1);
            _cd3 = new GRPC_NetworkVariable<int>(cooldownNetVarPrefix + 2);
            _cd4 = new GRPC_NetworkVariable<int>(cooldownNetVarPrefix + 3);
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            InitNetVars();
            
            if (!IsClient) return;

            for (var i = 0; i < _cooldowns.Length; i++)
            {
                var cd = _cooldowns[i];
                var temp = i;
                
                cd.OnValueChanged += (x, newValue) =>
                {
                    OnServerCooldownUpdated?.Invoke(temp, newValue);
                    if (newValue <= 0) OnServerCooldownEnded?.Invoke(temp);
                };
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            ResetNetworkVariables();
        }
        
        private void InitNetVars()
        {
            _cooldowns[0] = _cd1;
            _cooldowns[1] = _cd2;
            _cooldowns[2] = _cd3;
            _cooldowns[3] = _cd4;
            
            for (var i = 0; i < _cooldowns.Length; i++)
            {
                _cooldowns[i].Initialize();
            }
        }

        private void ResetNetworkVariables()
        {
            for (var i = 0; i < _cooldowns.Length; i++)
            {
                _cooldowns[i].Reset();
            }
        }
        
        public bool IsInCooldown(int index)
        {
            if (index < 0 || index >= _cooldowns.Length)
            {
                Debug.LogError($"Given cooldown index {index} is out of range");
                return false;
            }
            
            return _cooldowns[index].Value > 0;
        }

        /// <summary>
        /// Used by server to start cooldown.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        [Server]
        public void StartServerCooldown(int index, float time)
        {
            if (index < 0 || index >= _timers.Length) return;
            
            void TimerUpdate(float value)
            {
                _cooldowns[index].Value = (int)value;
            }

            void TimerEnd()
            {
                _cooldowns[index].Value = 0;
            }
            
            _cooldowns[index].Value = Mathf.RoundToInt(time);

            _timers[index].StartTimerWithUpdateCallback(this, time, TimerUpdate, TimerEnd, TimeType.Unscaled, ceiled: true);
        }
        
        /// <summary>
        /// Used as visual feedback for the client.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="time"></param>
        public void StartLocalCooldown(int index, float time)
        {
            if (index < 0 || index >= _timers.Length) return;
            
            #region Methods
            
            void TimerUpdate(float value)
            {
                OnLocalCooldownUpdated?.Invoke(index, value);
            }
            
            #endregion
            
            _timers[index].StartTimerWithUpdateCallback(this, time, TimerUpdate, timerType: TimeType.Unscaled, ceiled: true);
            
            OnLocalCooldownStarted?.Invoke(index, time);
        }
    }
}