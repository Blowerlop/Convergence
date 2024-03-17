using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project
{
    [Serializable]
    public abstract class StatBase : ICloneable
    {
        public abstract object Clone();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Value type: int, float, long, etc...</typeparam>
    [Serializable]
    public abstract class Stat<T> : StatBase where T : struct, IComparable, IFormattable, IConvertible
    {
        [SerializeField, OnValueChanged("SetValue")] private T _defaultValue;
        
        [SerializeField, ReadOnly] private T _value;
        public T value
        {
            get => _value;
            set
            {
                // Return if the value is equal to the current value
                if (_equalityComparer.Equals(_value, value)) return;
                
                _value = value;
                OnValueChanged?.Invoke(_value, _maxValue);
            }
        }

        [SerializeField] private T _maxValue;
        public T maxValue
        {
            get => _maxValue;
            set
            {
                if (_equalityComparer.Equals(_maxValue, value)) return;
                
                _maxValue = value;
                OnValueChanged?.Invoke(_value, _maxValue);
            }
        }

        // Value / MaxValue
        public event Action<T, T> OnValueChanged;
        
        private EqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
        
        
        public void SetToMaxValue()
        {
            value = maxValue;
        }

        public override object Clone()
        {
            return (Stat<T>)MemberwiseClone();
        }

        [Button]
        private void SetValue(T value)
        {
            this.value = value;
        }
    }
}