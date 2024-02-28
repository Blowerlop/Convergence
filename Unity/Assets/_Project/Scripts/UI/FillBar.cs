using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Project
{
    public class FillBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;

        [SerializeField] private float _fillAnimDuration = 0.25f;
        [SerializeField] private Ease _fillEase;

        private float _maxValue;
        
        private Sequence _fillSequence;

        private void InitSequence()
        {
            if(_fillSequence != null && _fillSequence.IsActive()) 
                _fillSequence.Kill();
            
            _fillSequence = DOTween.Sequence();
        }
        
        public void SetFillAmount(float normalizedValue, bool instant = false)
        {
            InitSequence();
            
            if (instant)
            {
                _fillImage.fillAmount = normalizedValue;
            }
            else
            {
                _fillSequence.Append(_fillImage.DOFillAmount(normalizedValue, _fillAnimDuration).SetEase(_fillEase));
            }
        }
        
        public void SetFillAmount(float current, float max, bool instant = false)
        {
            float normalized = current / max;
            SetFillAmount(normalized);
        }
    }
}