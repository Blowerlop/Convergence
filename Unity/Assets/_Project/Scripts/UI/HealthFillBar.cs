using System;

namespace Project
{
    public class HealthFillBar : FillBar
    {
        private PlayerStats _stats;
        
        private void Awake()
        {
            PlayerRefs.OnLocalPlayerSpawned += Setup;
            
            SetFillAmount(1);
        }

        private void OnDestroy()
        {
            PlayerRefs.OnLocalPlayerSpawned -= Setup;
            
            if (!_stats) return;
            
            _stats.OnHealthChanged -= OnHealthChanged;
        }
        
        private void Setup(PlayerRefs refs)
        {
            _stats = refs.GetStats();
            
            _stats.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            SetFillAmount(currentHealth, maxHealth);
        }
    }
}