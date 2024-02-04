using UnityEngine;

namespace Project
{
    public class HealthFillBar : FillBar
    {
        private PCStats _stats;
        
        private void Awake()
        {
            UserInstance.Me.OnPlayerLinked += Setup;
            
            SetFillAmount(1);
        }

        private void OnDestroy()
        {
            if (UserInstance.Me != null) UserInstance.Me.OnPlayerLinked -= Setup;
            
            if (!_stats) return;
            
            _stats.OnHealthChanged -= OnHealthChanged;
        }
        
        private void Setup(PlayerRefs refs)
        {
            if (refs.Stats is PCStats stats)
            {
                _stats = stats;
            }
            else
            {
                Debug.LogError("Trying to link a non-PCStats to HealthFillBar!");
                return;
            }

            _stats.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            SetFillAmount(currentHealth, maxHealth);
        }
    }
}