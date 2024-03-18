namespace Project
{
    public class PlayerStatFillBar<T> : FillBar where T : Stat<int>
    {
        private PlayerStats _stats;
        
        private void Awake()
        {
            UserInstance.Me.OnPlayerLinked += Setup;
            
            SetFillAmount(1);
        }

        private void OnDestroy()
        {
            if (UserInstance.Me != null) UserInstance.Me.OnPlayerLinked -= Setup;
            
            if (!_stats) return;
            
            _stats.Get<T>().OnValueChanged -= OnValueChanged;
        }
        
        private void Setup(PlayerRefs refs)
        {
            if (refs is not PCPlayerRefs _refs) return;
            
            _stats = _refs.Entity.Stats;

            if (_stats.isInitialized) OnStatsInitialized();
            else _stats.OnStatsInitialized += OnStatsInitialized;
        }
        
        private void OnStatsInitialized()
        {
            var stat = _stats.Get<T>();
            OnValueChanged(stat.value, stat.maxValue);
            stat.OnValueChanged += OnValueChanged;
            
            _stats.OnStatsInitialized -= OnStatsInitialized;
        }
        
        private void OnValueChanged(int currentValue, int maxValue)
        {
            SetFillAmount(currentValue, maxValue);
        }
    }
}