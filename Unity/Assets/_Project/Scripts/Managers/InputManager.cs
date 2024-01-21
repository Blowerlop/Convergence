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
    [DefaultExecutionOrder(-1)]
    public class InputManager : MonoSingleton<InputManager>
    {
        #region Variables
        private PlayerInputAction _inputAction;
        
        [HideInInspector] public string _defaultInputActionMap;
        public InputActionMap currentActionMap { get; private set; }
        public InputActionMap previousActionMap { get; private set; }
        
        [Title("Parameters")]
        [ShowInInspector, PropertyOrder(1), LabelText("Current Action Map")]
        #if UNITY_EDITOR
        private string _currentActionMapEditor
        {
            get
            {
                if (currentActionMap == null) return _defaultInputActionMap;
                return currentActionMap.name;
            }
        }
        #endif
        
        [Title("Inputs Handler")]
        public InputAction onMouseButton0 => _inputAction.Player.MouseButton0;
        public InputAction onMouseButton1 => _inputAction.Player.MouseButton1;
        public InputAction onConsoleKey => _inputAction.Persistant.Console;
        public InputAction onCenterCamera => _inputAction.Player.CenterCamera;
        public InputAction onLockCamera => _inputAction.Player.LockCamera;

        // Spells
        private InputAction[] _spellInputs;
        public event Action<int> OnSpellInputStarted; 
        public event Action<int> OnOnSpellInputCanceled; 
        
        [Title("Inputs value")] 
        [ShowInInspector] public Vector2 move;
        [ShowInInspector] public Vector2 look;
        
        #endregion
        
        
        #region Updates
        protected override void Awake()
        {
            dontDestroyOnLoad = false;
            base.Awake();
            
            _inputAction = new PlayerInputAction();
            
            InitInputAction();
            
            SwitchActionMap(_defaultInputActionMap);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var spellAction in _spellInputs)
            {
                spellAction.started -= SpellInputStarted;
                spellAction.canceled -= SpellInputCanceled;
            }
        }

        #endregion

        
        #region Methods
        private void InitInputAction()
        {
            // Move
            _inputAction.Player.Move.started += context => move = context.ReadValue<Vector2>();
            _inputAction.Player.Move.performed += context => move = context.ReadValue<Vector2>();
            _inputAction.Player.Move.canceled += _ => move = Vector2.zero;
            
            // Look
            _inputAction.Player.Look.started += context => look = context.ReadValue<Vector2>();
            _inputAction.Player.Look.performed += context => look = context.ReadValue<Vector2>();
            _inputAction.Player.Look.canceled += _ => look = Vector2.zero;
            
            // Spells
            _spellInputs = new[]
            {
                _inputAction.Player.Spell1,
                _inputAction.Player.Spell2,
                _inputAction.Player.Spell3,
                _inputAction.Player.Spell4
            };
            
            foreach (var spellAction in _spellInputs)
            {
                spellAction.started += SpellInputStarted;
                spellAction.canceled += SpellInputCanceled;
            }
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
        
        public void SwitchActionMap(InputActionMap actionMap)
        {
            if (actionMap == null)
            {
                Debug.LogError("Action map is null");
                return;
            }
            if (currentActionMap == actionMap && actionMap.enabled) return;

            _inputAction.Disable();

            Debug.Log(currentActionMap != null
                ? $"Switching action map : {currentActionMap.name} To {actionMap.name}"
                : $"Setting action map : {actionMap.name}");

            previousActionMap = currentActionMap;
            currentActionMap = actionMap;

            Utilities.StartWaitForFramesAndDoActionCoroutine(this, 1, () =>
            {
                currentActionMap.Enable();
                _inputAction.Persistant.Enable();
            });
        }

        private void SpellInputStarted(InputAction.CallbackContext _)
        {
            int spellIndex = Array.IndexOf(_spellInputs, _.action);
            OnSpellInputStarted?.Invoke(spellIndex);
        }
        
        private void SpellInputCanceled(InputAction.CallbackContext _)
        {
            int spellIndex = Array.IndexOf(_spellInputs, _.action);
            OnOnSpellInputCanceled?.Invoke(spellIndex);
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


            PlayerInputAction inputAction = new PlayerInputAction();

            SerializedProperty actionMapProperty = serializedObject.FindProperty("_defaultInputActionMap");
            if (actionMapProperty == null) return;
            // EditorGUILayout.PropertyField(actionMapProperty);

            ReadOnlyArray<InputActionMap> actionMaps = inputAction.asset.actionMaps;
            string[] array = new string[actionMaps.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = actionMaps[i].name;
                if (array[i] == actionMapProperty.stringValue) _index = i;
            }
            
            _index = EditorGUILayout.Popup("Default Action Map", _index, array);
            actionMapProperty.stringValue = array[_index];
            
            serializedObject.ApplyModifiedProperties();
            
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
    #endif
}