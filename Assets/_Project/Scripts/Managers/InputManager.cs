using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace Project
{
    public struct InputActionPattern<T> where T : struct
    {
        private readonly InputAction _inputAction;
        public T value;

        private InputActionPattern(InputAction inputAction)
        {
            value = default;
            
            if (inputAction == null)
            {
                Debug.LogError("Have you initialized this field in the constructor? You can initialize it in the Awake/Start/OnEnable method, after initializing the InputActionAsset.");
                _inputAction = null;
                return;
            }
            
            _inputAction = inputAction;
        }

        public InputActionPattern(InputAction inputAction, 
            Action<InputAction.CallbackContext> started = null, 
            Action<InputAction.CallbackContext> performed = null, 
            Action<InputAction.CallbackContext> canceled = null) : this(inputAction)
        {
            if (started != null)
            {
                _inputAction.started += started;
            }

            if (performed != null)
            {
                _inputAction.performed += performed;
            }

            if (canceled != null)
            {
                _inputAction.canceled += canceled;
            }
        }
    }
    
    
    
    public class InputManager : Singleton<InputManager>
    {
        #region Variables
        private PlayerInputAction _inputAction;
        
        [HideInInspector] public string _defaultInputActionMap;
        public InputActionMap currentActionMap { get; private set; } = null;
        #if UNITY_EDITOR
        [Title("Parameters")]
        [ShowInInspector, PropertyOrder(1), LabelText("Current Action Map")]
        private string _currentActionMapEditor
        {
            get
            {
                if (currentActionMap == null) return _defaultInputActionMap;
                return currentActionMap.name;
            }
        }
        #endif
        
        // Inputs Handler
        private InputActionPattern<Vector2> _move;
        private InputActionPattern<bool> _fire;
        private InputActionPattern<bool> _console;

        // Inputs Value
        public Vector2 move => _move.value;
        public bool fire => _fire.value;
        public bool console => _console.value;
        #endregion
        
        
        #region Updates
        protected override void Awake()
        {
            keepAlive = false;
            base.Awake();
            if (isBeingDestroyed) return;
            
            _inputAction = new PlayerInputAction();
            
            InitInputAction();
            
            // SwitchActionMap(_defaultInputActionMap.ToString());
            SwitchActionMap(_defaultInputActionMap);
        }
        
        // private void OnEnable()
        // {
        //     SwitchActionMapForced(currentActionMap);
        // }
        //
        // private void OnDisable()
        // {
        //     if (isBeingDestroyed) return;
        //     
        //     _inputAction.Disable();
        // }
        #endregion

        
        #region Methods
        private void InitInputAction()
        {
            _move = new InputActionPattern<Vector2>(_inputAction.Player.Move,
                performed: context => _move.value = context.ReadValue<Vector2>(),
                canceled: context => _move.value = Vector2.zero);

            _fire = new InputActionPattern<bool>(_inputAction.Player.Fire,
                started: context => _fire.value = true,
                performed: context =>
                {
                    if (_fire.value)
                    {
                        StartCoroutine(Utilities.WaitForEndOfFrameAndDoActionCoroutine(() => _fire.value = false));
                    }
                },
                canceled: context => _fire.value = false);
            
            _console = new InputActionPattern<bool>(_inputAction.Persistant.Console,
                started: context => _console.value = true,
                performed: context =>
                {
                    if (_console.value)
                    {
                        StartCoroutine(Utilities.WaitForEndOfFrameAndDoActionCoroutine(() => _console.value = false));
                    }
                },
                canceled: context => _console.value = false);
        }
        
        
        public void SwitchActionMap(InputActionMap actionMap)
        {
            if (actionMap == null) return;
            if (currentActionMap == actionMap && actionMap.enabled) return;

            _inputAction.Disable();
        
            if (currentActionMap != null) Debug.Log($"Switching action map : {currentActionMap.name} To {actionMap.name}");
            else Debug.Log($"Setting action map : {actionMap.name}");
            currentActionMap = actionMap;
            currentActionMap.Enable();
             
            _inputAction.Persistant.Enable();
        }
        
        [Title("Button"),PropertyOrder(0), Button]
        public void SwitchActionMap(string actionMapName)
        {
            InputActionMap actionMap = _inputAction.asset.FindActionMap(actionMapName);
            if (actionMap == null)
            {
                Debug.LogError($"There is no action map with the name : {actionMapName}");
                return;
            }

            SwitchActionMap(actionMap);
        }
        
        #endregion
    }


    #if UNITY_EDITOR
    [CustomEditor(typeof(InputManager))]
    public class InputManagerEditor : OdinEditor
    {
        private int _index;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            InputManager t = target as InputManager;
            if (t == null) return;


            PlayerInputAction _inputAction = new PlayerInputAction();

            SerializedProperty actionMapProperty = serializedObject.FindProperty("_defaultInputActionMap");
            if (actionMapProperty == null) return;
            // EditorGUILayout.PropertyField(actionMapProperty);

            ReadOnlyArray<InputActionMap> actionMaps = _inputAction.asset.actionMaps;
            string[] array = new string[actionMaps.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = actionMaps[i].name;
                if (array[i] == actionMapProperty.stringValue) _index = i;
            }
            
            _index = EditorGUILayout.Popup("Default Action Map", _index, array);
            actionMapProperty.stringValue = array[_index];
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
