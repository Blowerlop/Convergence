using UnityEngine;
using UnityEngine.InputSystem;

namespace Project
{
    public class MenuManager : MonoSingleton<MenuManager>
    {
        [SerializeField] private GameObject _menu; 
        
        
        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
        }

        private void OnEnable()
        {
            InputManager.instance.onEscapeKey.performed += Toggle;
        }
        
        private void OnDisable()
        {
            InputManager.instance.onEscapeKey.performed -= Toggle;
        }


        private void Toggle(InputAction.CallbackContext _)
        {
            bool active = !_menu.activeSelf;
            _menu.SetActive(active);
            
            if (active) InputManager.instance.SwitchActionMap("UI");
            else InputManager.instance.SwitchActionMap(InputManager.instance.previousActionMap);
        }
    }
}
