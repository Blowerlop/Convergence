using System;

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
            UserInstance.Me.OnPlayerLinked -= Setup;
            
            if (!_stats) return;
            
            _stats.OnHealthChanged -= OnHealthChanged;
        }
        
        private void Setup(PlayerRefs refs)
        {
            _stats = refs.Stats;
            
            _stats.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            SetFillAmount(currentHealth, maxHealth);
        }
    }
}