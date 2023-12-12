using System;
using Unity.Netcode;
using UnityEngine;

namespace Project.Spells
{
    /// <summary>
    /// Used by clients and server to determine if a spell can be casted.
    /// </summary>
    public class CooldownController : MonoBehaviour
    {
        private readonly bool[] _cooldownInProgress = new bool[SpellData.CharacterSpellsCount];
        
        private readonly Timer[] _timers = new Timer[SpellData.CharacterSpellsCount];
        
        public event Action<int> OnCooldownStarted; 
        public event Action<int, float> OnCooldownUpdated; 
        public event Action<int> OnCooldownEnded;

        private void Awake()
        {
            for(int i = 0; i < _timers.Length; i++)
            {
                _timers[i] = new Timer();
            }
        }

        public bool IsInCooldown(int index)
        {
            if (index < 0 || index >= _cooldownInProgress.Length)
            {
                Debug.LogError($"Given cooldown index {index} is out of range");
                return false;
            }

            return _cooldownInProgress[index];
        }
        
        public void StartCooldown(int index, float time)
        {
            if (index < 0 || index >= _timers.Length) return;
            
            #region Methods
            
            void TimerUpdate(float value)
            {
                OnCooldownUpdated?.Invoke(index, value);
            }

            void TimerEnd()
            {
                _cooldownInProgress[index] = false;
                OnCooldownEnded?.Invoke(index);
            }
            
            #endregion
            
            _cooldownInProgress[index] = true;

            _timers[index].StartTimerWithUpdateCallback(this, time, TimerUpdate, TimerEnd, TimeType.Unscaled);
            
            OnCooldownStarted?.Invoke(index);
        }
    }
}