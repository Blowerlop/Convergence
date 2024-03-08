using System;

namespace Project
{
    public class AttackDamage : IStat<int>
    {
        public int DefaultValue { get; set; } = 20;
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public void SetToMaxValue()
        {
            Value = MaxValue;
        }

        public event Action<int, int> OnValueChanged;
    }
}