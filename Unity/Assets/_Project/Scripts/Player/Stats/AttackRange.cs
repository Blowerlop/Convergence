using System;

namespace Project
{
    public class AttackRange : IStat<float>
    {
        public float DefaultValue { get; set; }
        public float Value { get; set; }
        public float MaxValue { get; set; }
        public void SetToMaxValue()
        {
            Value = MaxValue;
        }

        public event Action<float, float> OnValueChanged;
    }
} 