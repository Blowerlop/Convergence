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
        [SerializeField] private CooldownController cooldowns;
        [SerializeField] private int id;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI tmp;
        [SerializeField] private Image img;

        private float maxTime;
        
        private void Awake()
        {
            cooldowns.OnLocalCooldownStarted += OnCooldownStarted;
            cooldowns.OnLocalCooldownUpdated += OnCooldownUpdated;
            
            cooldowns.OnServerCooldownEnded += OnCooldownEnded;
        }

        private void OnCooldownStarted(int index, float time)
        {
            if (index != id) return;
            
            group.DOFade(1, 0);
            img.fillAmount = 1;
            tmp.text = Mathf.RoundToInt(time).ToString();

            maxTime = time;
        }

        private void OnCooldownUpdated(int index, float value)
        {
            if (index != id) return;
            
            tmp.text = value.ToString(CultureInfo.InvariantCulture);
            img.fillAmount = maxTime / value;
        }
        
        private void OnCooldownEnded(int index)
        {
            if (index != id) return;
            
            group.DOFade(0, 0);
        }
    }
}