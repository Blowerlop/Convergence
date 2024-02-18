using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Project._Project.TESTT_REBIND
{
    public class InputSettingsUI : MonoBehaviour
    {
        [field: Title("Binds")]
        [field: SerializeField, LabelText("Input Action Reference")] public InputActionReference inputActionReference { get; private set; }
        [SerializeField, HideInInspector] private int _bindingIndex;
        [ShowInInspector, ReadOnly] private InputBinding _inputBinding;
        private string _actionName;
    
        [Title("Visual")]
        [SerializeField] private InputBinding.DisplayStringOptions _displayStringOptions;
        
        [Title("Settings")]
        [SerializeField] private bool _excludeMouse = true;
        
        [Header("UI Fields")]
        [SerializeField] private TMP_Text _actionText;
        [SerializeField] private Button _rebindButton;
        [SerializeField] private TMP_Text _rebindText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _clearButton;

        
        private void OnEnable()
        {
            if(inputActionReference != null)
            {
                // InputSettingsManager.LoadBindingOverride(_actionName);
                GetBindingInfo();
                UpdateUI();
            }
            
            _rebindButton.onClick.AddListener(DoRebind);
            _resetButton.onClick.AddListener(ResetBinding);
            _clearButton.onClick.AddListener(ClearRebind);
            
            InputSettingsManager.onRebindComplete += UpdateUI;
            InputSettingsManager.onRebindCanceled += UpdateUI;
        
#if UNITY_EDITOR
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Settings")
            {
                Utilities.StartWaitForFramesAndDoActionCoroutine(this, 1, () =>
                {
                    GetBindingInfo();
                    UpdateUI();
                });
            }
#endif
        }

        private void OnDisable()
        {
            _rebindButton.onClick.RemoveListener(DoRebind);
            _resetButton.onClick.RemoveListener(ResetBinding);
            _clearButton.onClick.RemoveListener(ClearRebind);
            
            InputSettingsManager.onRebindComplete -= UpdateUI;
            InputSettingsManager.onRebindCanceled -= UpdateUI;
        }

        private void OnValidate()
        {
            if (inputActionReference == null) return; 
            
            GetBindingInfo();
            UpdateUI();
        }

        
        private void GetBindingInfo()
        {
            if (inputActionReference.action == null) return;
            
            _actionName = inputActionReference.action.name;

            if(inputActionReference.action.bindings.Count > _bindingIndex)
            {
                _inputBinding = inputActionReference.action.bindings[_bindingIndex];
            }
        }

        private void UpdateUI()
        {
            if (_actionText != null)
                _actionText.text = _actionName;

            if(_rebindText != null)
            {
                if (Application.isPlaying)
                {
                    _rebindText.text = InputSettingsManager.GetBindingName(_actionName, _bindingIndex, _displayStringOptions);
                }
                else
                    _rebindText.text = inputActionReference.action.GetBindingDisplayString(_bindingIndex, _displayStringOptions);
            }
        }

        private void DoRebind()
        {
            InputSettingsManager.StartRebind(_actionName, _bindingIndex, _rebindText, _excludeMouse);
        }

        private void ResetBinding()
        {
            InputSettingsManager.ResetBinding(_actionName, _bindingIndex);
            UpdateUI();
        }

        private void ClearRebind()
        {
            InputSettingsManager.ClearBindingOverride(_actionName, _bindingIndex);
            UpdateUI();
        }
    }
    
    
#if UNITY_EDITOR
    [CustomEditor(typeof(InputSettingsUI))]
    public class InputSettingsUIEditor : OdinEditor
    {
        private int _index;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InputSettingsUI t = target as InputSettingsUI;
            if (t == null) return;

            if (t.inputActionReference == null) return;
            
            SerializedProperty bindingIndexProperty = serializedObject.FindProperty("_bindingIndex");

            ReadOnlyArray<InputBinding> bindings = t.inputActionReference.action.bindings;
            string[] array = new string[bindings.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = bindings[i].path;
                if (i == bindingIndexProperty.intValue) _index = i;
            }

            _index = EditorGUILayout.Popup("Binding", _index, array);
            bindingIndexProperty.intValue = _index;
            
            serializedObject.ApplyModifiedProperties();
            
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
    #endif  
}
