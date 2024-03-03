using System;

namespace Project
{
    public interface IStat
    {
        public int DefaultValue { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }
        public void SetToMaxValue();
        public event Action<int, int> OnValueChanged;
    }
}