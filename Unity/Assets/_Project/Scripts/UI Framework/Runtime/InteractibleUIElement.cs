using Project._Project.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Project.Scripts.UIFramework
{
    public abstract class InteractibleUIElement : UIElement, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        // Settings
        [SerializeField, BoxGroup("Group/Settings/Sound")] private bool _playHoverSound = true;
        [SerializeField, BoxGroup("Group/Settings/Sound")] private bool _playClickSound = true;

        
        // Events
        [SerializeField, TabGroup("Group", "Events")] public UnityEvent _onHover = new UnityEvent();
        [SerializeField, TabGroup("Group", "Events")] public UnityEvent _onUnHover = new UnityEvent();
        [SerializeField, TabGroup("Group", "Events")] public UnityEvent _onClickDown = new UnityEvent();
        [SerializeField, TabGroup("Group", "Events")] public UnityEvent _onClickUp = new UnityEvent();
        [SerializeField, TabGroup("Group", "Events")] public UnityEvent _onClick = new UnityEvent();
        
        [SerializeField, FoldoutGroup("Group/Events/Animations")] public UnityEvent _onHoverAnimation = new UnityEvent();
        [SerializeField, FoldoutGroup("Group/Events/Animations")] public UnityEvent _onUnHoverAnimation = new UnityEvent();
        [SerializeField, FoldoutGroup("Group/Events/Animations")] public UnityEvent _onClickDownAnimation = new UnityEvent();
        [SerializeField, FoldoutGroup("Group/Events/Animations")] public UnityEvent _onClickUpAnimation = new UnityEvent();
        [SerializeField, FoldoutGroup("Group/Events/Animations")] public UnityEvent _onClickAnimation = new UnityEvent();
        

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (_playHoverSound) PlayHoverSound();
            _onHoverAnimation.Invoke();
            _onHover.Invoke();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _onUnHoverAnimation.Invoke();
            _onUnHover.Invoke();
        }
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_playClickSound) PlayClickSound();
            _onClickDownAnimation.Invoke();
            _onClickDown.Invoke();
        }
        
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _onClickUpAnimation.Invoke();
            _onClickUp.Invoke();
        }
        
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            _onClickAnimation.Invoke();
            _onClick.Invoke();
        }

        protected void PlayHoverSound() => SoundManager.instance.PlayGlobalSound("hover", "sfx", SoundManager.EventType.SFX);
        protected void PlayClickSound() => SoundManager.instance.PlayGlobalSound("click", "sfx", SoundManager.EventType.SFX);
    }
}