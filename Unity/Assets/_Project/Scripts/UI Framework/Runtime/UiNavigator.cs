using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Project.Scripts.UIFramework
{
    public class UiNavigator : MonoBehaviour, IPointerClickHandler
    {
        public static List<UiNavigator> _navigators = new List<UiNavigator>();
        [ShowInInspector, ReadOnly] private List<UiNavigable> _navigation = new List<UiNavigable>();


        private void OnEnable()
        {
            if (_navigators.Any() == false)
            {
                CursorManager.Request(CursorLockMode.Confined, true);
            }
            
            _navigators.Add(this);

            InputManager.instance.onEscapeKey.performed += CloseElement;
        }

        private void OnDisable()
        {
            if (_navigators.Any() == false)
            {
                CursorManager.Release();
            }
            
            _navigators.Remove(this);

            if (InputManager.IsInstanceAlive())
            {
                InputManager.instance.onEscapeKey.performed -= CloseElement;
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            CloseElement(default);
        }

        private bool IsNavigating()
        {
            return _navigation.Count > 0;
        }
        
        public void Register(UiNavigable element)
        {
            _navigation.Add(element);
            _navigation = _navigation.OrderBy(x => x.hierarchyDepth).ToList();
        }
        
        public void UnRegister(UiNavigable element)
        {
            _navigation.Remove(element);
        }

        private void CloseElement(InputAction.CallbackContext _)
        {
            if (IsNavigating() == false)
            {
                Debug.LogError("Not navigating while trying to navigate. This is a bug.");
                return;
            }

            _navigation[^1].Close();
        }
    }
}