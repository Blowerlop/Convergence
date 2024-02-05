using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project._Project.Scripts.UI.Settings
{
    public class SliderExtended : Slider
    {
        [SerializeField] private SliderEvent m_OnPointerUp = new SliderEvent();
        public SliderEvent onPointerUp
        {
            get => m_OnPointerUp;
            set => m_OnPointerUp = value;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            
            onPointerUp.Invoke(value);
        }
    }
}