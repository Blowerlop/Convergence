using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Spells
{
    public class SpellUI : MonoBehaviour
    {
        private CooldownController _cooldowns;
        [SerializeField] private int id;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private Image img;

        private float _maxTime;
        
        private void Awake()
        {
            PlayerRefs.OnLocalPlayerSpawned += Setup;
        }

        private void OnDestroy()
        {
            PlayerRefs.OnLocalPlayerSpawned -= Setup;
            
            _cooldowns.OnLocalCooldownStarted -= OnCooldownStarted;
            _cooldowns.OnLocalCooldownUpdated -= OnCooldownUpdated;
            
            _cooldowns.OnServerCooldownEnded -= OnCooldownEnded;
        }

        private void Setup(PlayerRefs refs)
        {
            _cooldowns = refs.GetCooldownController(PlayerPlatform.Pc);
            
            _cooldowns.OnLocalCooldownStarted += OnCooldownStarted;
            _cooldowns.OnLocalCooldownUpdated += OnCooldownUpdated;
            
            _cooldowns.OnServerCooldownEnded += OnCooldownEnded;
            
            group.DOFade(0, 0);
        }
        
        private void OnCooldownStarted(int index, float time)
        {
            if (index != id) return;
            
            group.DOFade(1, 0);
            img.fillAmount = 1;
            tmp.text = Mathf.RoundToInt(time).ToString();

            _maxTime = time;
        }

        private void OnCooldownUpdated(int index, float value)
        {
            if (index != id) return;
            
            tmp.text = value.ToString(CultureInfo.InvariantCulture);
            img.fillAmount = _maxTime / value;
        }
        
        private void OnCooldownEnded(int index)
        {
            if (index != id) return;
            
            group.DOFade(0, 0);
        }
    }
}