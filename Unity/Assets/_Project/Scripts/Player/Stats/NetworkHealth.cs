using System;
using Sirenix.OdinInspector;

namespace Project
{
    public class NetworkHealth : NStat<HealthStat, int>
    {
        [ShowInInspector] public override GRPC_NetworkVariable<int> _nValue { get; set; } = new GRPC_NetworkVariable<int>("Health", 1);
        [ShowInInspector] public override GRPC_NetworkVariable<int> _nMaxValue { get; set; } = new GRPC_NetworkVariable<int>("MaxHealth", 1);
        
        
        public override int Clamp(int current, int min, int max)
        {
            if (min > max || max < min) throw new InvalidOperationException();

            if (current <= min) return min;
            if (current >= max) return max;

            return current;
        }
    }
}