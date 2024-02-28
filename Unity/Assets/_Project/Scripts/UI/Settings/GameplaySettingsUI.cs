using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Project.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace Project._Project.Scripts.UI.Settings
{
    public class GameplaySettingsUI : MonoBehaviour
    {

#if UNITY_EDITOR
        [ShowInInspector, HideIf("@gameplaySettings == null")] private bool value
        {
            get => gameplaySettings is { value: true };
            // ReSharper disable once ValueParameterNotUsed
            set => ToggleState();
        }
#endif

        [SerializeField, HideInInspector] private string key;
        [HideInInspector] public GameplaySettings gameplaySettings;
        [ClearOnReload] private static IEnumerable<GameplaySettings> _gameplaySettingsFields;

        [SerializeField, Required] private Button _button;


        private void Start()
        {
            LinkReference();
        }

        private void OnEnable()
        {
            _button.onClick.AddListenerExtended(ToggleState);
        }
        
        private void OnDisable()
        {
            _button.onClick.RemoveListenerExtended(ToggleState);
        }


        private void ToggleState()
        {
            gameplaySettings.value = !gameplaySettings.value;
            Debug.Log(gameplaySettings.value);
        }

        private void LinkReference()
        {
            if (_gameplaySettingsFields == null)
            {
                FieldInfo[] fields = typeof(GameplaySettingsManager).GetFields(BindingFlags.Public | BindingFlags.Static);

                _gameplaySettingsFields = fields
                    .Where(field => field.FieldType == typeof(GameplaySettings))
                    .Select(x => x.GetValue(null))
                    .Cast<GameplaySettings>();
            }

            gameplaySettings = _gameplaySettingsFields.First(x => x.key == key);
        }
    }


    #if UNITY_EDITOR
    [CustomEditor(typeof(GameplaySettingsUI))]
    public class GameplaySettingsUIEditor : OdinEditor
    {
        private GameplaySettings[] _gameplaySettingsContainers;
        private string[] _keys;
        private string _key;


        protected override void OnEnable()
        {
            base.OnEnable();
            
            FieldInfo[] fields = typeof(GameplaySettingsManager).GetFields(BindingFlags.Public | BindingFlags.Static);
            
            _gameplaySettingsContainers = fields
                .Where(field => field.FieldType == typeof(GameplaySettings))
                .Select(x => x.GetValue(null))
                .Cast<GameplaySettings>()
                .ToArray();

            _keys = _gameplaySettingsContainers
                .Select(x => x.key)
                .ToArray();

            _key = _keys.First();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameplaySettingsUI t = target as GameplaySettingsUI;
            if (t == null) return;
            
            int index = -1;
            for (int i = 0; i < _keys.Length; i++)
            {
                if (_keys[i] == _key)
                {
                    index = i;
                    break;
                }
            }
            
            index = EditorGUILayout.Popup("Key", index, _keys);
            _key = _keys[index];
            
            t.gameplaySettings = _gameplaySettingsContainers[index];

            var keySerialized = serializedObject.FindProperty("key");
            keySerialized.stringValue = _key;
            serializedObject.ApplyModifiedProperties(); 
        }
    }
    #endif
}