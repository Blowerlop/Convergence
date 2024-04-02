using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Project.Scripts.UIFramework
{
    public class NavigationUI : MonoBehaviour, IPointerClickHandler
    {
        public static List<NavigationUI> _navigators = new List<NavigationUI>();
        [ShowInInspector] private List<NavigationElementUI> _navigation = new List<NavigationElementUI>();


        private void OnEnable()
        {
            if (_navigators.Any() == false)
            {
                // Show cursor
            }
            
            _navigators.Add(this);
        }

        private void OnDisable()
        {
            if (_navigators.Any() == false)
            {
                // Revert cursor
            }
            
            _navigators.Remove(this);
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseElement();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CloseElement();
        }

        private bool IsNavigating()
        {
            return _navigation.Count > 0;
        }
        
        public void Register(NavigationElementUI element)
        {
            _navigation.Add(element);
            _navigation = _navigation.OrderBy(x => x.hierarchyDepth).ToList();
        }
        
        public void UnRegister(NavigationElementUI element)
        {
            _navigation.Remove(element);
        }

        private void CloseElement()
        {
            if (IsNavigating() == false)
            {
                Debug.LogError("Not navigation while trying to navigate. This is a bug.");
                return;
            }

            _navigation[^1].Close();
        }
    }
}