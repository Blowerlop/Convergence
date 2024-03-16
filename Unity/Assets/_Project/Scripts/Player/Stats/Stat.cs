using System;
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
                _maxValue = value;
                OnValueChanged?.Invoke(_value, _maxValue);
            }
        }

        // Value / MaxValue
        public event Action<T, T> OnValueChanged;
        
        
        public void SetToMaxValue()
        {
            value = maxValue;
        }

        public override object Clone()
        {
            return (Stat<T>)MemberwiseClone();
        }

        private void SetValue(T value)
        {
            _value = value;
        }
    }
}