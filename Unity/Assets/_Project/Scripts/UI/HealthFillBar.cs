namespace Project
{
    public class HealthFillBar : FillBar
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
            
            _stats.health.OnValueChanged -= OnHealthChanged;
        }
        
        private void Setup(PlayerRefs refs)
        {
            if (refs is not PCPlayerRefs _refs) return;
            
            _stats = _refs.Entity.Stats;
            
            _stats.health.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            SetFillAmount(currentHealth, maxHealth);
        }
    }
}