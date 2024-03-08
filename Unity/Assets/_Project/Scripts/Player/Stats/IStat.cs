using System;

namespace Project
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Value type: int, float, long, etc...</typeparam>
    public interface IStat<T> where T : struct, IComparable, IFormattable, IConvertible 
    {
        public T DefaultValue { get; set; }
        public T Value { get; set; }
        public T MaxValue { get; set; }
        public void SetToMaxValue();
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event Action<T, T> OnValueChanged;
    }
}